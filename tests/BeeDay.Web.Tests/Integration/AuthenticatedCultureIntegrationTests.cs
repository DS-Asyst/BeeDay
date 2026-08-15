using System.Net;
using System.Security.Claims;
using BeeDay.Application.Common.Contracts;
using BeeDay.Domain.Enums;
using BeeDay.Web.Services.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the Sprint 23.3 precedence rule at the two places it is applied — the real
/// POST /auth/login pipeline, and the per-request OnValidatePrincipal path that closes the gap
/// for a session authenticated purely by a persistent "BeeDay.Auth" cookie (never repassing
/// through /auth/login) — plus the guarantee that logout never touches BeeDay.Culture.
/// </summary>
public sealed class AuthenticatedCultureIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task ExistingAuthenticatedSession_WithoutCultureCookie_AppliesThePortugueseAccountLanguage()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "persistent-session-pt@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!");
        await SetAccountLanguageAsync(user.Id, UserLanguage.Portuguese, cancellationToken);

        using var response = await GetAccountWithForgedAuthCookieAsync(user.Id, cultureCookie: null, cancellationToken);
        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Minha Conta", html, StringComparison.Ordinal);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, cookie => cookie.StartsWith("BeeDay.Culture=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExistingAuthenticatedSession_WithoutCultureCookie_AppliesTheEnglishAccountLanguage()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "persistent-session-en@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!"); // English by default.

        using var response = await GetAccountWithForgedAuthCookieAsync(user.Id, cultureCookie: null, cancellationToken);
        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("My Account", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExistingAuthenticatedSession_WithAlreadyValidCultureCookie_IsCheapAndNeverConvergesTheAccount()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "persistent-session-explicit@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!"); // English account.

        using var response = await GetAccountWithForgedAuthCookieAsync(user.Id, "c=pt-BR|uic=pt-BR", cancellationToken);
        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        // The explicit cookie still wins over the (English) account for this request/render...
        Assert.Contains("Minha Conta", html, StringComparison.Ordinal);
        // ...and, being the cheap/no-write path, the response never reissues BeeDay.Culture.
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            Assert.DoesNotContain(cookies, cookie => cookie.StartsWith("BeeDay.Culture=", StringComparison.Ordinal));
        }

        // Unlike the login path, this per-request path never converges the account either — it is
        // read-only by design, scoped to applying a language, not reconciling one.
        var stillEnglish = await factory.FindUserAsync(email);
        Assert.Equal(UserLanguage.English, stillEnglish!.Language);
    }

    [Fact]
    public async Task Login_WithoutCultureCookie_AppliesThePortugueseAccountLanguage()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "no-cookie-portuguese@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!");
        await SetAccountLanguageAsync(user.Id, UserLanguage.Portuguese, cancellationToken);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginCookie = await LoginAsync(client, email, "Password123!", cancellationToken);

        Assert.NotNull(loginCookie);
        Assert.Equal("c=pt-BR|uic=pt-BR", Uri.UnescapeDataString(loginCookie!.Value.ToString()));
    }

    [Fact]
    public async Task Login_WithoutCultureCookie_AppliesTheEnglishAccountLanguage()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "no-cookie-english@beeday.invalid";
        await factory.SeedConfirmedUserAsync(email, "Password123!"); // User.Create defaults to English.

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginCookie = await LoginAsync(client, email, "Password123!", cancellationToken);

        Assert.NotNull(loginCookie);
        Assert.Equal("c=en-US|uic=en-US", Uri.UnescapeDataString(loginCookie!.Value.ToString()));
    }

    [Fact]
    public async Task Login_WithExplicitPortugueseCookie_OverEnglishAccount_KeepsPortugueseAndConvergesTheAccount()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "explicit-pt-over-en-account@beeday.invalid";
        await factory.SeedConfirmedUserAsync(email, "Password123!"); // English by default.

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");
        var loginCookie = await LoginAsync(client, email, "Password123!", cancellationToken);

        // The cookie already carried the explicit choice, so the synchronizer has no reason to
        // reissue it — the session simply keeps using the one already in effect.
        Assert.Null(loginCookie);
        var loginPageAfterAuth = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Entrar", loginPageAfterAuth, StringComparison.Ordinal);

        // ...and the account converges to it for future sessions.
        var updated = await factory.FindUserAsync(email);
        Assert.Equal(UserLanguage.Portuguese, updated!.Language);
    }

    [Fact]
    public async Task Login_WithExplicitEnglishCookie_OverPortugueseAccount_KeepsEnglishAndConvergesTheAccount()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "explicit-en-over-pt-account@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!");
        await SetAccountLanguageAsync(user.Id, UserLanguage.Portuguese, cancellationToken);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=en-US|uic=en-US");
        var loginCookie = await LoginAsync(client, email, "Password123!", cancellationToken);

        Assert.Null(loginCookie);
        var loginPageAfterAuth = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Welcome back", loginPageAfterAuth, StringComparison.Ordinal);

        var updated = await factory.FindUserAsync(email);
        Assert.Equal(UserLanguage.English, updated!.Language);
    }

    [Fact]
    public async Task Login_AlreadyConsistentCookieAndAccount_NoCultureCookieIsReissued()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "already-consistent@beeday.invalid";
        var user = await factory.SeedConfirmedUserAsync(email, "Password123!");
        await SetAccountLanguageAsync(user.Id, UserLanguage.Portuguese, cancellationToken);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        // Cookie and account already agreed on pt-BR: no write was needed, so no new
        // BeeDay.Culture Set-Cookie is issued on the response (the browser's existing one already
        // carries the correct value).
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            Assert.DoesNotContain(cookies, cookie => cookie.StartsWith("BeeDay.Culture=", StringComparison.Ordinal));
        }

        var updated = await factory.FindUserAsync(email);
        Assert.Equal(UserLanguage.Portuguese, updated!.Language);
    }

    [Fact]
    public async Task Logout_DoesNotClearTheCultureCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "logout-preserves-culture@beeday.invalid";
        await factory.SeedConfirmedUserAsync(email, "Password123!");

        // Same client/cookie container throughout: set the culture explicitly (as Home's switcher
        // would), log in, then log out — never touching DefaultRequestHeaders manually, so the
        // container is the single source of truth for every cookie the same way a real browser is.
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var cultureToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["__RequestVerificationToken"] = cultureToken
            }),
            cancellationToken);

        await LoginAsync(client, email, "Password123!", cancellationToken);

        var logoutToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        var response = await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = logoutToken }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            Assert.DoesNotContain(cookies, cookie => cookie.StartsWith("BeeDay.Culture=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Logout_ThenHome_StillRendersThePreviouslyChosenCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var email = "logout-then-home-pt@beeday.invalid";
        await factory.SeedConfirmedUserAsync(email, "Password123!");

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var cultureToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["__RequestVerificationToken"] = cultureToken
            }),
            cancellationToken);

        await LoginAsync(client, email, "Password123!", cancellationToken);

        var logoutToken = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        await client.PostAsync(
            "/auth/logout",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = logoutToken }),
            cancellationToken);

        var homeHtml = await client.GetStringAsync("/", cancellationToken);
        Assert.Contains("Comece agora", homeHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("Get started", homeHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Reaches an authenticated page with only a forged "BeeDay.Auth" cookie — never a real
    /// POST /auth/login — the same shape as a browser reconnecting via a long-lived "remember me"
    /// session. Optionally attaches a raw BeeDay.Culture cookie value to test the "already
    /// explicit" branch of the per-request path.
    /// </summary>
    private async Task<HttpResponseMessage> GetAccountWithForgedAuthCookieAsync(
        Guid userId, string? cultureCookie, CancellationToken cancellationToken)
    {
        var authCookie = factory.CreateRawAuthCookie(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(BeeDayClaimTypes.SessionVersion, "1")
        ]);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, "/account");
        var cookieHeader = cultureCookie is null
            ? $"BeeDay.Auth={authCookie}"
            : $"BeeDay.Auth={authCookie}; BeeDay.Culture={cultureCookie}";
        request.Headers.Add("Cookie", cookieHeader);

        return await client.SendAsync(request, cancellationToken);
    }

    private async Task SetAccountLanguageAsync(Guid userId, UserLanguage language, CancellationToken cancellationToken)
    {
        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await repository.UpdateAsync(userId, user => user.UpdatePreferences(language, user.Theme), cancellationToken);
    }

    private static async Task<SetCookieHeaderValue?> LoginAsync(HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        var response = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        if (!response.Headers.TryGetValues("Set-Cookie", out var rawCookies))
        {
            return null;
        }

        var parsed = SetCookieHeaderValue.ParseList([.. rawCookies]);
        return parsed.SingleOrDefault(cookie => cookie.Name.ToString() == "BeeDay.Culture");
    }
}
