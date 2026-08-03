using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Confirms /auth/login and /auth/logout actually reject requests missing, carrying an invalid,
/// or carrying a mismatched-context antiforgery token, and accept requests carrying a genuinely
/// valid one — the behavior docs/security/01-security-baseline.md asked to be verified with
/// integration tests.
/// </summary>
public sealed class AntiforgeryIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task Login_WithoutAntiforgeryToken_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("no-token-login@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "no-token-login@beeday.invalid",
                ["password"] = "Password123!"
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidAntiforgeryToken_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("invalid-token-login@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Fetching the page establishes the antiforgery cookie; the field value is garbage.
        await client.GetStringAsync("/login", cancellationToken);

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "invalid-token-login@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = "not-a-real-token"
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithTokenFromAnotherClientContext_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("cross-context-login@beeday.invalid", "Password123!");

        // Client A's antiforgery cookie never reaches Client B; pairing B's cookie with A's
        // field value must fail, since the field must match the specific cookie it was issued with.
        using var clientA = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var clientB = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var tokenFromA = await AntiforgeryTestHelper.GetTokenAsync(clientA, "/login", cancellationToken);
        await clientB.GetStringAsync("/login", cancellationToken);

        var response = await clientB.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "cross-context-login@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = tokenFromA
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidAntiforgeryToken_Succeeds()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("with-token-login@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "with-token-login@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertSetsCookie(response, "BeeDay.Auth");
    }

    [Fact]
    public async Task Login_FailureResponseDoesNotRevealAccountExistence()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("exists-wrong-password@beeday.invalid", "Password123!");
        using var existingClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var missingClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var existingToken = await AntiforgeryTestHelper.GetTokenAsync(existingClient, "/login", cancellationToken);
        var existingResponse = await existingClient.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "exists-wrong-password@beeday.invalid",
                ["password"] = "WrongPassword!",
                ["__RequestVerificationToken"] = existingToken
            }),
            cancellationToken);

        var missingToken = await AntiforgeryTestHelper.GetTokenAsync(missingClient, "/login", cancellationToken);
        var missingResponse = await missingClient.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "does-not-exist@beeday.invalid",
                ["password"] = "WrongPassword!",
                ["__RequestVerificationToken"] = missingToken
            }),
            cancellationToken);

        Assert.Equal(existingResponse.StatusCode, missingResponse.StatusCode);
        Assert.Equal(existingResponse.Headers.Location, missingResponse.Headers.Location);
    }

    [Fact]
    public async Task Logout_WithoutAntiforgeryToken_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("no-token-logout@beeday.invalid", "Password123!", cancellationToken);

        var response = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["returnUrl"] = string.Empty }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidAntiforgeryToken_Succeeds()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("with-token-logout@beeday.invalid", "Password123!", cancellationToken);
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/daily", cancellationToken);

        var response = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Logout_GetRequest_DoesNotLogOut()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("get-logout@beeday.invalid", "Password123!", cancellationToken);

        // No POST /auth/logout endpoint responds to GET at all — confirm it doesn't sign the user out.
        await client.GetAsync("/auth/logout", cancellationToken);

        var dashboardResponse = await client.GetAsync("/daily", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, dashboardResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_ExternalReturnUrl_IsIgnoredInFavorOfLocalDefault()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("logout-external-returnurl@beeday.invalid", "Password123!", cancellationToken);
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/daily", cancellationToken);

        var response = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
                ["returnUrl"] = "https://evil.example.com/steal"
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.False(response.Headers.Location!.IsAbsoluteUri, "Logout must never redirect to an external URL.");
    }

    [Fact]
    public async Task Login_ExternalReturnUrl_IsIgnoredInFavorOfLocalDefault()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("login-external-returnurl@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "login-external-returnurl@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token,
                ["returnUrl"] = "https://evil.example.com/phish"
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.False(response.Headers.Location!.IsAbsoluteUri, "Login must never redirect to an external URL.");
    }

    private static void AssertSetsCookie(HttpResponseMessage response, string cookieName)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies), "Response did not set any cookies.");
        Assert.Contains(cookies, cookie => cookie.StartsWith(cookieName + "=", StringComparison.Ordinal));
    }
}
