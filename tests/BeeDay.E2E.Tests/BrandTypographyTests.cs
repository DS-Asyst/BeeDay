using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class BrandTypographyTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Theory]
    [InlineData(390, 844)]
    [InlineData(1280, 800)]
    public async Task PublicTypographyGuidelinesLoadQualifiedFontsWithoutClippingOrOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/brand/typography");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Typography with purpose", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator(".brand-typography__display-sample")).ToHaveTextAsync("beeday");
        await Expect(Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "Typography" })).ToHaveAttributeAsync("href", "/brand/typography");

        await Page.EvaluateAsync("() => document.fonts.ready");
        Assert.True(await Page.EvaluateAsync<bool>("() => document.fonts.check('48px Coiny', 'beeday ações')"));
        Assert.True(await Page.EvaluateAsync<bool>("() => document.fonts.check('16px Nunito', 'Product interface')"));

        var displaySample = Page.Locator(".brand-typography__display-sample");
        Assert.Contains("Coiny", await displaySample.EvaluateAsync<string>("element => getComputedStyle(element).fontFamily"), StringComparison.Ordinal);
        Assert.Equal("rgb(82, 71, 249)", await displaySample.EvaluateAsync<string>("element => getComputedStyle(element).color"));
        Assert.True(await Page.Locator(".brand-typography__accented-sample").EvaluateAsync<bool>(
            "element => { const range = document.createRange(); range.selectNodeContents(element); const text = range.getBoundingClientRect(); const box = element.getBoundingClientRect(); return text.top >= box.top - 1 && text.bottom <= box.bottom + 1; }"));
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task PortugueseGuidelinesPreserveLocalizationBrandCasingAndAccentedGlyphs()
    {
        await Page.SetViewportSizeAsync(390, 844);
        await GotoAsync("/brand/typography");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" }).ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Tipografia com propósito", Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.Locator(".brand-typography__accented-sample")).ToHaveTextAsync("Pequenas ações, grandes conquistas.");
        await Expect(Page.GetByRole(AriaRole.Contentinfo).GetByRole(AriaRole.Link, new() { Name = "Tipografia" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".brand-typography__display-sample")).ToHaveTextAsync("beeday");
        Assert.False(await Page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }
}
