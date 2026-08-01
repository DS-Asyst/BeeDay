using System.Net;
using System.Security.Claims;
using LevelUp.Application.Features.Identity.Commands;
using LevelUp.Application.Features.Identity.Requests;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Requests;
using LevelUp.Web.Services.Authentication;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LevelUp.Web.Tests.Integration;

/// <summary>
/// Validates the real <c>OnValidatePrincipal</c> handler in Program.cs — never Domain or handler
/// unit tests in isolation — against every previously-issued-cookie scenario Sprint 12.5
/// introduced: malformed/missing claims, a nonexistent or deactivated user, a stale SessionVersion
/// after a password change or reset, and the positive control (current version stays valid).
/// </summary>
public sealed class SessionInvalidationIntegrationTests(LevelUpWebApplicationFactory factory)
    : IClassFixture<LevelUpWebApplicationFactory>
{
    [Fact]
    public async Task ForgedCookie_WithCurrentSessionVersion_IsAccepted()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("session-current-version@levelup.invalid", "Password123!");

        var response = await GetWithForgedCookieAsync(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(LevelUpClaimTypes.SessionVersion, "1")
            ],
            cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ForgedCookie_MissingSessionVersionClaim_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("session-missing-claim@levelup.invalid", "Password123!");

        var response = await GetWithForgedCookieAsync(
            [new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())],
            cancellationToken);

        AssertRejected(response);
    }

    [Fact]
    public async Task ForgedCookie_WithNonNumericSessionVersionClaim_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var user = await factory.SeedConfirmedUserAsync("session-invalid-claim@levelup.invalid", "Password123!");

        var response = await GetWithForgedCookieAsync(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(LevelUpClaimTypes.SessionVersion, "not-a-number")
            ],
            cancellationToken);

        AssertRejected(response);
    }

    [Fact]
    public async Task ForgedCookie_MissingNameIdentifierClaim_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;

        var response = await GetWithForgedCookieAsync(
            [new Claim(LevelUpClaimTypes.SessionVersion, "1")],
            cancellationToken);

        AssertRejected(response);
    }

    [Fact]
    public async Task ForgedCookie_ForNonexistentUser_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;

        var response = await GetWithForgedCookieAsync(
            [
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(LevelUpClaimTypes.SessionVersion, "1")
            ],
            cancellationToken);

        AssertRejected(response);
    }

    [Fact]
    public async Task RealCookie_ForDeactivatedUser_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("session-deactivated@levelup.invalid", "Password123!", cancellationToken);
        var user = await factory.FindUserAsync("session-deactivated@levelup.invalid");

        await factory.DeactivateUserAsync(user!.Id);

        var afterDeactivation = await client.GetAsync("/daily", cancellationToken);
        AssertRejected(afterDeactivation);
    }

    [Fact]
    public async Task RealCookie_AfterPasswordChange_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("session-password-change@levelup.invalid", "Password123!", cancellationToken);
        var user = await factory.FindUserAsync("session-password-change@levelup.invalid");

        using (var scope = factory.CreateAuthenticatedScope(user!.Id, user.SessionVersion))
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(
                new ChangeCurrentUserPasswordCommand(new ChangeUserPasswordRequest("Password123!", "NewPassword123!", "NewPassword123!")),
                cancellationToken);
        }

        var afterPasswordChange = await client.GetAsync("/daily", cancellationToken);
        AssertRejected(afterPasswordChange);
    }

    [Fact]
    public async Task RealCookie_AfterPasswordReset_IsRejectedAndOldPasswordStopsWorking()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("session-password-reset@levelup.invalid", "Password123!", cancellationToken);
        var user = await factory.FindUserAsync("session-password-reset@levelup.invalid");
        var rawToken = await factory.IssuePasswordResetTokenAsync(user!.Id);

        using (var scope = factory.Services.CreateScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(
                new ResetPasswordCommand(new ResetPasswordRequest(rawToken, "NewPassword123!", "NewPassword123!")),
                cancellationToken);
        }

        var afterReset = await client.GetAsync("/daily", cancellationToken);
        AssertRejected(afterReset);

        using var freshClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var oldPasswordAttempt = await PostLoginAsync(freshClient, "session-password-reset@levelup.invalid", "Password123!", cancellationToken);
        Assert.Contains("error=invalid", oldPasswordAttempt.Headers.Location!.ToString(), StringComparison.Ordinal);

        var newPasswordAttempt = await PostLoginAsync(freshClient, "session-password-reset@levelup.invalid", "NewPassword123!", cancellationToken);
        Assert.False(newPasswordAttempt.Headers.Location?.ToString().Contains("error=invalid", StringComparison.Ordinal) ?? false);
    }

    private async Task<HttpResponseMessage> GetWithForgedCookieAsync(IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        var cookieValue = factory.CreateRawAuthCookie(claims);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/daily");
        request.Headers.Add("Cookie", $"LevelUp.Auth={cookieValue}");
        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
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

    private static void AssertRejected(HttpResponseMessage response)
    {
        // OnValidatePrincipal rejecting the principal makes the request fall through as
        // unauthenticated, which the cookie handler's OnRedirectToLogin turns into a redirect —
        // the same outward behavior as never having a cookie at all.
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location!.ToString(), StringComparison.Ordinal);
    }
}
