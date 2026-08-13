using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>Sprint 21.8 icon contracts verified in Chromium, including external SVG sprite use.</summary>
public sealed class IconSystemTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task PublicHome_RendersLucideOutlineIconsFromTheOfficialSprite()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        var icons = Page.Locator("svg.beeday-icon:not([data-icon-category='social'])");
        await Expect(icons.First).ToBeVisibleAsync();

        var count = await icons.CountAsync();
        Assert.True(count > 0);

        for (var index = 0; index < count; index++)
        {
            var icon = icons.Nth(index);
            var href = await icon.Locator("use").GetAttributeAsync("href");
            Assert.StartsWith("/icons/sprite.svg#", href, StringComparison.Ordinal);

            var presentation = await icon.EvaluateAsync<IconPresentation>("""
                element => {
                    const style = getComputedStyle(element);
                    return { fill: style.fill, stroke: style.stroke, strokeWidth: style.strokeWidth };
                }
                """);

            Assert.Equal("none", presentation.Fill);
            Assert.NotEqual("none", presentation.Stroke);
            Assert.Equal("2px", presentation.StrokeWidth);
        }

        var spriteResponse = await Page.APIRequest.GetAsync("/icons/sprite.svg");
        Assert.True(spriteResponse.Ok);
        Assert.Contains("stroke=\"currentColor\"", await spriteResponse.TextAsync(), StringComparison.Ordinal);
    }

    private sealed class IconPresentation
    {
        public string Fill { get; set; } = string.Empty;
        public string Stroke { get; set; } = string.Empty;
        public string StrokeWidth { get; set; } = string.Empty;
    }
}
