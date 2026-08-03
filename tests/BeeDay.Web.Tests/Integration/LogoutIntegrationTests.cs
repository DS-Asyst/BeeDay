using System.Net;
using Microsoft.Net.Http.Headers;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Covers logout scenarios not already exercised by AntiforgeryIntegrationTests (missing/valid
/// token, GET doesn't log out, external returnUrl rejected): cookie removal, loss of access to
/// protected resources, repeat-logout safety, and the distinction between "sign out this one
/// cookie" (logout) and "invalidate every session" (SessionVersion, covered separately).
/// </summary>
public sealed class LogoutIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task Logout_ClearsTheAuthCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("logout-clears-cookie@beeday.invalid", "Password123!", cancellationToken);
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/daily", cancellationToken);

        var response = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }),
            cancellationToken);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var rawCookies));
        var authCookie = SetCookieHeaderValue.ParseList([.. rawCookies]).Single(c => c.Name.ToString() == "BeeDay.Auth");
        // ASP.NET Core clears a cookie by re-issuing it with an empty value and an Expires date
        // in the past, instructing the browser to delete it immediately.
        Assert.Equal(string.Empty, authCookie.Value.ToString());
        Assert.NotNull(authCookie.Expires);
        Assert.True(authCookie.Expires!.Value < DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Logout_RevokesAccessToProtectedPagesOnTheSameClient()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("logout-revokes-access@beeday.invalid", "Password123!", cancellationToken);

        var beforeLogout = await client.GetAsync("/daily", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, beforeLogout.StatusCode);

        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/daily", cancellationToken);
        await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = token }),
            cancellationToken);

        var afterLogout = await client.GetAsync("/daily", cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.Contains("/login", afterLogout.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Logout_CalledTwice_DoesNotError()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = await factory.CreateAuthenticatedClientAsync("logout-twice@beeday.invalid", "Password123!", cancellationToken);

        var firstToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/daily", cancellationToken);
        var firstLogout = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = firstToken }),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, firstLogout.StatusCode);

        // /auth/logout requires authorization; a second, now-unauthenticated attempt is expected
        // to be redirected to the login challenge rather than throwing a server error.
        var secondToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        var secondLogout = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = secondToken }),
            cancellationToken);

        Assert.True(
            secondLogout.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Found,
            $"Repeated logout must not error; got {secondLogout.StatusCode}.");
    }

    [Fact]
    public async Task Logout_OnOneClient_DoesNotInvalidateAnotherClientsSessionForTheSameUser()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;

        using var deviceA = await factory.CreateAuthenticatedClientAsync("logout-is-local@beeday.invalid", "Password123!", cancellationToken);
        using var deviceB = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var tokenB = await AntiforgeryTestHelper.GetTokenAsync(deviceB, "/login", cancellationToken);
        await deviceB.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "logout-is-local@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = tokenB
            }),
            cancellationToken);

        var tokenA = await AntiforgeryTestHelper.GetTokenAsync(deviceA, "/daily", cancellationToken);
        await deviceA.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = tokenA }),
            cancellationToken);

        // Device A's session is gone; Device B's independent session (same user, same
        // SessionVersion, different cookie) is untouched — logout is per-cookie, not per-user.
        var deviceAAfter = await deviceA.GetAsync("/daily", cancellationToken);
        var deviceBAfter = await deviceB.GetAsync("/daily", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, deviceAAfter.StatusCode);
        Assert.Equal(HttpStatusCode.OK, deviceBAfter.StatusCode);
    }
}
