using Microsoft.Playwright;

namespace BeeDay.E2E.Tests;

// EPIC 28, Sprint 28.9: deliberately independent of PlaywrightAppFixture/E2EWebApplicationFactory -
// rendering a raw transactional-email HTML string has nothing to do with the live Blazor Server app
// or a real route, so this skips the (comparatively expensive) full ASP.NET Core host boot entirely.
// One Chromium instance shared across EmailClientCompatibilityTests' test methods.
public sealed class EmailPreviewPlaywrightFixture : IAsyncLifetime
{
    public IBrowser Browser { get; private set; } = null!;

    private IPlaywright playwright = null!;

    public async ValueTask InitializeAsync()
    {
        playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.CloseAsync();
        playwright.Dispose();
    }
}
