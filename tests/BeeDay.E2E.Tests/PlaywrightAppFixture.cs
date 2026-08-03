using Microsoft.Playwright;

namespace LevelUp.E2E.Tests;

/// <summary>
/// One Chromium instance and one hosted app per test class (via IClassFixture), shared across that
/// class's test methods to avoid repeated browser/host startup cost. Each individual test still gets
/// its own isolated BrowserContext/Page — see <see cref="E2ETestBase"/>.
/// </summary>
public sealed class PlaywrightAppFixture : IAsyncLifetime
{
    public E2EWebApplicationFactory Factory { get; } = new();
    public IBrowser Browser { get; private set; } = null!;
    public string ServerAddress => Factory.ServerAddress;

    private IPlaywright playwright = null!;

    public async ValueTask InitializeAsync()
    {
        // Forces WebApplicationFactory to build and start the host, binding the real Kestrel port.
        // The `Server` property itself throws NotSupportedException once UseKestrel is active (it
        // only makes sense for TestServer); CreateClient() is the supported way to trigger startup.
        using (Factory.CreateClient())
        {
        }

        playwright = await Playwright.CreateAsync();
        Browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        // A real Blazor Server round trip (SignalR circuit handshake, server-side render, real
        // Kestrel) is slower and more variable than a mocked/in-memory test; Playwright's 5s
        // assertion default is tuned for typical SPA/static-content latency, not this. 10s absorbs
        // that without masking a genuine failure (still fails loudly, just later).
        Assertions.SetDefaultExpectTimeout(10_000);
    }

    public async ValueTask DisposeAsync()
    {
        await Browser.CloseAsync();
        playwright.Dispose();
        await Factory.DisposeAsync();
    }
}
