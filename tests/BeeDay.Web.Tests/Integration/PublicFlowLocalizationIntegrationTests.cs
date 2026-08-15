using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the public flow Epic 23 / Sprint 23.2 needed to prove: a visitor's chosen culture,
/// set on the Home page through the official POST /culture/set mechanism, survives navigation to
/// Login, a reload, and — critically — the POST /auth/login + redirect boundary itself. None of
/// this relies on PublicHomeLanguageState/PublicHomeCopy (removed this Sprint); it is exercised
/// entirely through the real HTTP pipeline established in Sprint 23.1.
/// </summary>
public sealed class PublicFlowLocalizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task HomePage_WithoutCultureCookie_RendersEnglishContent()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Build a better day", html, StringComparison.Ordinal);
        Assert.Contains("Get started", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomePage_WithPortugueseCultureCookie_RendersPortugueseContent()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Construa um dia melhor", html, StringComparison.Ordinal);
        Assert.Contains("Comece agora", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Build a better day", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginPage_WithoutCultureCookie_RendersEnglishContent()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/login", cancellationToken);

        Assert.Contains("Welcome back", html, StringComparison.Ordinal);
        Assert.Contains("Sign In", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginPage_WithPortugueseCultureCookie_RendersPortugueseContent()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=pt-BR|uic=pt-BR");

        var html = await client.GetStringAsync("/login", cancellationToken);

        Assert.Contains("Bem-vindo de volta", html, StringComparison.Ordinal);
        Assert.Contains("Entrar", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Welcome back", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginPage_WithUnsupportedCultureCookie_FallsBackToEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", "BeeDay.Culture=c=fr-FR|uic=fr-FR");

        var html = await client.GetStringAsync("/login", cancellationToken);

        Assert.Contains("Welcome back", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomeSelectsPortuguese_LoginStillPortuguese_AndReloadPreservesIt()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/", cancellationToken);

        // Mirrors exactly what PublicLanguageSwitcher's form posts when a visitor picks
        // Português on Home, with returnUrl="/" as the switcher itself would send there.
        var setResponse = await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["returnUrl"] = "/",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, setResponse.StatusCode);
        Assert.Equal("/", setResponse.Headers.Location!.ToString());

        // Same client (same cookie jar) navigating to Login exactly as "I already have an
        // account" would — including via a forceLoad, which this HTTP-level test already is:
        // there is no in-memory circuit state to lose here, only the cookie.
        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Bem-vindo de volta", loginHtml, StringComparison.Ordinal);

        // Reload.
        var loginHtmlAfterReload = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Bem-vindo de volta", loginHtmlAfterReload, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomeSelectsEnglish_LoginStillEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/", cancellationToken);

        var setResponse = await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "en-US",
                ["returnUrl"] = "/",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, setResponse.StatusCode);

        var loginHtml = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Welcome back", loginHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_WithPortugueseCulture_DoesNotTouchTheCultureCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("culture-boundary-cookie@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        var loginResponse = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "culture-boundary-cookie@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            Assert.DoesNotContain(cookies, cookie => cookie.StartsWith("BeeDay.Culture=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task HomePortuguese_ThenLogin_ThenAuthenticate_CultureSurvivesThePostAndRedirect()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await factory.SeedConfirmedUserAsync("culture-boundary-survives@beeday.invalid", "Password123!");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        // 1. Visitor picks Português (as if from Home).
        await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);

        // 2. Login is reached and rendered in Portuguese.
        var loginHtmlBeforeAuth = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Bem-vindo de volta", loginHtmlBeforeAuth, StringComparison.Ordinal);

        // 3. Real authentication happens (same client/cookie jar as the culture cookie).
        var loginResponse = await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = "culture-boundary-survives@beeday.invalid",
                ["password"] = "Password123!",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        // 4. Effective culture after the authentication POST + redirect boundary is still
        // Portuguese — proven by revisiting the (still AllowAnonymous) Login page with the same,
        // now-authenticated client: nothing in the authentication pipeline resets the cookie.
        var loginHtmlAfterAuth = await client.GetStringAsync("/login", cancellationToken);
        Assert.Contains("Bem-vindo de volta", loginHtmlAfterAuth, StringComparison.Ordinal);
    }
}
