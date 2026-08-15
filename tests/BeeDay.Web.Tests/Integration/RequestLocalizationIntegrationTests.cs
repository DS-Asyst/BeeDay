using BeeDay.Web.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Verifies the RequestLocalizationOptions wiring in Program.cs and that the real HTTP pipeline
/// resolves resources to the correct culture end-to-end via GET / — proving culture resolution
/// survives an ordinary request/response cycle, not just a component-level bUnit render.
/// </summary>
public sealed class RequestLocalizationIntegrationTests(BeeDayWebApplicationFactory factory)
    : IClassFixture<BeeDayWebApplicationFactory>
{
    [Fact]
    public void RequestLocalizationOptions_SupportsEnglishAndPortuguese()
    {
        var options = factory.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

        Assert.Contains(options.SupportedCultures!, culture => culture.Name == "en-US");
        Assert.Contains(options.SupportedCultures!, culture => culture.Name == "pt-BR");
        Assert.Contains(options.SupportedUICultures!, culture => culture.Name == "en-US");
        Assert.Contains(options.SupportedUICultures!, culture => culture.Name == "pt-BR");
    }

    [Fact]
    public void RequestLocalizationOptions_DefaultsToEnglish()
    {
        var options = factory.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

        Assert.Equal("en-US", options.DefaultRequestCulture.Culture.Name);
        Assert.Equal("en-US", options.DefaultRequestCulture.UICulture.Name);
    }

    [Fact]
    public void RequestLocalizationOptions_ResolvesCultureOnlyThroughItsOwnCookie()
    {
        var options = factory.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;

        var provider = Assert.Single(options.RequestCultureProviders!);
        var cookieProvider = Assert.IsType<CookieRequestCultureProvider>(provider);
        Assert.Equal(BeeDayCultures.CookieName, cookieProvider.CookieName);
    }

    [Fact]
    public async Task HomePage_WithoutCultureCookie_RendersTheEnglishFallback()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Be Better Every Day", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomePage_WithPortugueseCultureCookie_RendersThePortugueseResource()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{BeeDayCultures.CookieName}=c=pt-BR|uic=pt-BR");

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Seja melhor a cada dia", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Be Better Every Day", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomePage_WithUnsupportedCultureCookie_FallsBackToEnglish()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{BeeDayCultures.CookieName}=c=fr-FR|uic=fr-FR");

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Be Better Every Day", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HomePage_WithMalformedCultureCookie_FallsBackToEnglishInsteadOfFailing()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"{BeeDayCultures.CookieName}=not-a-real-cookie-value");

        var html = await client.GetStringAsync("/", cancellationToken);

        Assert.Contains("Be Better Every Day", html, StringComparison.Ordinal);
    }
}
