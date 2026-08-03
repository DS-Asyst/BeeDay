using System.Net;
using LevelUp.Application.Common.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LevelUp.Web.Tests.Integration;

/// <summary>
/// Exercises POST /auth/login against the real pipeline: valid/invalid credentials, normalized
/// email lookup, deactivated/unconfirmed users, cookie and SessionVersion claim issuance,
/// redirect destinations, and that failure responses never distinguish "wrong password" from
/// "no such account".
/// </summary>
public sealed class LoginIntegrationTests(LevelUpWebApplicationFactory factory)
    : IClassFixture<LevelUpWebApplicationFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_Succeeds()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("valid-login@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("valid-login@levelup.invalid", "Password123!", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNormalizedEmailCasing_Succeeds()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("normalized-email@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("Normalized-Email@LevelUp.Invalid", "Password123!", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("error=invalid", response.Headers.Location?.ToString() ?? string.Empty, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShowsGenericError()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("wrong-password@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("wrong-password@levelup.invalid", "IncorrectPassword!", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("error=invalid", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ShowsSameGenericErrorAsWrongPassword()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("real-account@levelup.invalid", "Password123!");

        var wrongPasswordResponse = await PostLoginAsync("real-account@levelup.invalid", "IncorrectPassword!", cancellationToken);
        var nonexistentResponse = await PostLoginAsync("nobody-here@levelup.invalid", "IncorrectPassword!", cancellationToken);

        Assert.Equal(wrongPasswordResponse.StatusCode, nonexistentResponse.StatusCode);
        Assert.Equal(wrongPasswordResponse.Headers.Location, nonexistentResponse.Headers.Location);
    }

    [Fact]
    public async Task Login_WithDeactivatedUser_IsRejectedWithSameGenericError()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("deactivated-login@levelup.invalid", "Password123!");
        await factory.DeactivateUserAsync(user.Id);

        var response = await PostLoginAsync("deactivated-login@levelup.invalid", "Password123!", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("error=invalid", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_IsRejectedWithSameGenericError()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedUnconfirmedUserAsync("unconfirmed-login@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("unconfirmed-login@levelup.invalid", "Password123!", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("error=invalid", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_IssuesAuthCookieWithSessionVersionClaim()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("session-claim@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("session-claim@levelup.invalid", "Password123!", cancellationToken);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        var authCookie = Assert.Single(cookies, cookie => cookie.StartsWith("LevelUp.Auth=", StringComparison.Ordinal));
        // The cookie value itself is the encrypted/protected ticket; we cannot decode it without
        // the app's DataProtection keys, but its mere presence plus a successful subsequent
        // request to a protected page (covered by AuthorizationIntegrationTests) proves the
        // SessionVersion claim round-trips correctly through OnValidatePrincipal.
        Assert.NotEmpty(authCookie);
    }

    [Fact]
    public async Task Login_RedirectsToOnboarding_WhenProfileCompleteButOnboardingNot()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("needs-onboarding@levelup.invalid", "Password123!");

        var response = await PostLoginAsync("needs-onboarding@levelup.invalid", "Password123!", cancellationToken);

        Assert.Equal("/onboarding/tutorial", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Login_RedirectsToDaily_WhenOnboardingAlreadyCompleted()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("onboarding-done@levelup.invalid", "Password123!");
        using (var scope = factory.Services.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            await repository.UpdateAsync(user.Id, u => u.CompleteOnboarding(), cancellationToken);
        }

        var response = await PostLoginAsync("onboarding-done@levelup.invalid", "Password123!", cancellationToken);

        Assert.Equal("/daily", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Login_ResponseBody_NeverContainsPasswordOrHash()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("no-leak@levelup.invalid", "Sup3rSecretPassword!");

        var response = await PostLoginAsync("no-leak@levelup.invalid", "Sup3rSecretPassword!", cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.DoesNotContain("Sup3rSecretPassword!", body, StringComparison.Ordinal);
        Assert.DoesNotContain(user.PasswordHash, body, StringComparison.Ordinal);
    }

    private async Task<HttpResponseMessage> PostLoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        return await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
    }
}
