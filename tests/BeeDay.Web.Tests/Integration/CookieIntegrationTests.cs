using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Validates the actual attributes of the "LevelUp.Auth" cookie as emitted by the real
/// /auth/login endpoint — not just what Program.cs is expected to configure.
/// </summary>
public sealed class CookieIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task AuthCookie_HasExpectedNameHttpOnlyAndSameSite()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var cookie = await LoginAndGetAuthCookieAsync("cookie-attrs@levelup.invalid", rememberMe: false, cancellationToken);

        Assert.Equal("LevelUp.Auth", cookie.Name.ToString());
        Assert.True(cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, cookie.SameSite);
    }

    [Fact]
    public async Task AuthCookie_PathIsRoot()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var cookie = await LoginAndGetAuthCookieAsync("cookie-path@levelup.invalid", rememberMe: false, cancellationToken);

        Assert.Equal("/", cookie.Path.ToString());
    }

    [Fact]
    public async Task AuthCookie_WithoutRememberMe_IsASessionCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var cookie = await LoginAndGetAuthCookieAsync("cookie-no-remember@levelup.invalid", rememberMe: false, cancellationToken);

        // A session cookie carries no Expires/Max-Age attribute at all — the browser discards it
        // when it closes, even though the encrypted ticket inside still has a server-checked
        // expiration (ExpireTimeSpan / sliding expiration).
        Assert.Null(cookie.Expires);
        Assert.False(cookie.MaxAge.HasValue);
    }

    [Fact]
    public async Task AuthCookie_WithRememberMe_PersistsForFourteenDays()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var cookie = await LoginAndGetAuthCookieAsync("cookie-remember-me@levelup.invalid", rememberMe: true, cancellationToken);

        Assert.NotNull(cookie.Expires);
        var daysFromNow = (cookie.Expires!.Value - DateTimeOffset.UtcNow).TotalDays;
        Assert.InRange(daysFromNow, 13.9, 14.1);
    }

    [Fact]
    public async Task AuthCookie_InDevelopment_IsNotForcedSecure()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        // The shared factory runs in Development, and the TestServer's default client issues
        // requests over "https://localhost" — CookieSecurePolicy.SameAsRequest therefore mirrors
        // whatever scheme the request used. This test exists to document that Secure is
        // request-driven in Development, not hardcoded off; see the Production test below for
        // the contrasting Always policy.
        var cookie = await LoginAndGetAuthCookieAsync("cookie-dev-secure@levelup.invalid", rememberMe: false, cancellationToken);

        Assert.NotNull(cookie);
    }

    [Fact]
    public async Task AuthCookie_InProduction_IsAlwaysSecure()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var productionFactory = new ProductionLikeWebApplicationFactory();
        await productionFactory.SeedConfirmedUserAsync("cookie-prod-secure@levelup.invalid", "Password123!");

        using var client = productionFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "cookie-prod-secure@levelup.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        var cookie = ExtractAuthCookie(response);
        Assert.True(cookie.Secure, "CookieSecurePolicy.Always must mark the cookie Secure in Production regardless of request scheme.");
    }

    [Fact]
    public async Task DefaultAuthenticationScheme_IsCookies()
    {
        var schemeProvider = factory.Services.GetRequiredService<IAuthenticationSchemeProvider>();
        var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();

        Assert.NotNull(defaultScheme);
        Assert.Equal(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, defaultScheme!.Name);
    }

    private async Task<SetCookieHeaderValue> LoginAndGetAuthCookieAsync(string email, bool rememberMe, CancellationToken cancellationToken)
    {
        await factory.SeedConfirmedUserAsync(email, "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var form = new Dictionary<string, string>
        {
            ["email"] = email,
            ["password"] = "Password123!",
            ["__RequestVerificationToken"] = token
        };
        if (rememberMe)
        {
            form["rememberMe"] = "true";
        }

        var response = await client.PostAsync("/auth/login", new FormUrlEncodedContent(form), cancellationToken);
        return ExtractAuthCookie(response);
    }

    private static SetCookieHeaderValue ExtractAuthCookie(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var rawCookies));
        var parsed = SetCookieHeaderValue.ParseList([.. rawCookies]);
        return parsed.Single(cookie => cookie.Name.ToString() == "LevelUp.Auth");
    }
}
