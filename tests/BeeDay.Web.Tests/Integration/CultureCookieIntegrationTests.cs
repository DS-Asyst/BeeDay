using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises POST /culture/set against the real pipeline — the mechanism later Sprints (Home
/// switcher, Settings) will call to persist the visitor's/user's chosen UI culture. See
/// BeeDayCultures for the supported list, default, and cookie name this endpoint enforces.
/// </summary>
public sealed class CultureCookieIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public async Task SetCulture_WithSupportedCulture_SetsTheCultureCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", returnUrl: null, cancellationToken);

        var cookie = ExtractCultureCookie(response);
        Assert.Equal("c=pt-BR|uic=pt-BR", Uri.UnescapeDataString(cookie.Value.ToString()));
    }

    [Fact]
    public async Task SetCulture_Cookie_IsHttpOnlyAndSameSiteLax()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", returnUrl: null, cancellationToken);

        var cookie = ExtractCultureCookie(response);
        Assert.True(cookie.HttpOnly);
        Assert.Equal(SameSiteMode.Lax, cookie.SameSite);
    }

    [Fact]
    public async Task SetCulture_Cookie_IsDistinctFromTheAuthenticationCookie()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", returnUrl: null, cancellationToken);

        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var rawCookies));
        var parsed = SetCookieHeaderValue.ParseList([.. rawCookies]);
        Assert.DoesNotContain(parsed, cookie => cookie.Name.ToString() == "BeeDay.Auth");
    }

    [Fact]
    public async Task SetCulture_Cookie_PersistsForAboutOneYear()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", returnUrl: null, cancellationToken);

        var cookie = ExtractCultureCookie(response);
        Assert.NotNull(cookie.Expires);
        var daysFromNow = (cookie.Expires!.Value - DateTimeOffset.UtcNow).TotalDays;
        Assert.InRange(daysFromNow, 364, 366);
    }

    [Fact]
    public async Task SetCulture_WithUnsupportedCulture_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("fr-FR", returnUrl: null, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetCulture_WithoutAntiforgeryToken_IsRejected()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string> { ["culture"] = "pt-BR" }),
            cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetCulture_WithoutReturnUrl_RedirectsHome()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", returnUrl: null, cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SetCulture_WithLocalReturnUrl_RedirectsThere()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", "/account", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/account", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SetCulture_WithExternalReturnUrl_IsIgnoredInFavorOfHome()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var response = await PostSetCultureAsync("pt-BR", "https://evil.example.com/steal", cancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.False(response.Headers.Location!.IsAbsoluteUri, "Culture selection must never redirect to an external URL.");
    }

    [Fact]
    public async Task SetCulture_ThenANewRequest_UsesThePersistedCulture()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var setResponse = await client.PostAsync(
            "/culture/set",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["culture"] = "pt-BR",
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, setResponse.StatusCode);

        // Same HttpClient (same CookieContainer) issuing a brand-new request — the cookie set
        // above must be the one thing deciding the rendered language, nothing page-local.
        var homeHtml = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Seja melhor a cada dia", homeHtml, StringComparison.Ordinal);
    }

    private async Task<HttpResponseMessage> PostSetCultureAsync(string culture, string? returnUrl, CancellationToken cancellationToken)
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);

        var form = new Dictionary<string, string>
        {
            ["culture"] = culture,
            ["__RequestVerificationToken"] = token
        };
        if (returnUrl is not null)
        {
            form["returnUrl"] = returnUrl;
        }

        return await client.PostAsync("/culture/set", new FormUrlEncodedContent(form), cancellationToken);
    }

    private static SetCookieHeaderValue ExtractCultureCookie(HttpResponseMessage response)
    {
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var rawCookies));
        var parsed = SetCookieHeaderValue.ParseList([.. rawCookies]);
        return parsed.Single(cookie => cookie.Name.ToString() == "BeeDay.Culture");
    }
}
