using Microsoft.Playwright;

namespace BeeDay.E2E.Tests;

public sealed class ZZZManualVisualQaTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task ScreenshotRawSvgFullSize()
    {
        await Page.SetViewportSizeAsync(1920, 1100);
        await GotoAsync("/");
        var img = Page.Locator(".app-footer__wave img");
        await img.ScrollIntoViewIfNeededAsync();
        await Page.WaitForFunctionAsync("() => { const img = document.querySelector('.app-footer__wave img'); return img && img.complete && img.naturalHeight > 0; }");
        await Page.EvaluateAsync("() => { const img = document.querySelector('.app-footer__wave img'); img.style.maxHeight = 'none'; img.style.maxWidth = 'none'; img.style.width = '1920px'; img.style.height = 'auto'; }");
        await img.ScreenshotAsync(new() { Path = "C:/DevOps/MyHub/BeeDay/scratch-raw-svg.png" });
    }
}
