using Microsoft.Playwright;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Gives each test method its own isolated BrowserContext/Page (fresh cookies/storage per test,
/// per Playwright's own recommended isolation model) and captures a screenshot + trace only when
/// the test actually fails — using xunit v3's own documented TestContext.Current.TestState, not a
/// custom mechanism. Passing tests produce no artifacts at all.
/// </summary>
public abstract class E2ETestBase(PlaywrightAppFixture fixture) : IAsyncLifetime, IClassFixture<PlaywrightAppFixture>
{
    private static readonly string ArtifactsDirectory = Path.Combine(AppContext.BaseDirectory, "e2e-artifacts");

    protected PlaywrightAppFixture Fixture { get; } = fixture;
    protected IPage Page { get; private set; } = null!;

    private IBrowserContext context = null!;

    public async ValueTask InitializeAsync()
    {
        context = await Fixture.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = Fixture.ServerAddress });
        await context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true });
        Page = await context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
    {
        var failed = Xunit.TestContext.Current.TestState?.Result == Xunit.TestResult.Failed;

        if (failed)
        {
            var name = SanitizeFileName(Xunit.TestContext.Current.Test?.TestDisplayName ?? Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(ArtifactsDirectory);

            try
            {
                await Page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(ArtifactsDirectory, $"{name}.png"), FullPage = true });
            }
            catch (PlaywrightException)
            {
                // Best-effort only — a screenshot failing must never mask the real test failure.
            }

            await context.Tracing.StopAsync(new TracingStopOptions { Path = Path.Combine(ArtifactsDirectory, $"{name}.trace.zip") });
        }
        else
        {
            await context.Tracing.StopAsync();
        }

        await context.CloseAsync();
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
    }

    /// <summary>
    /// Navigates and waits for the network to settle. Blazor Server statically pre-renders a page
    /// on the initial response, then a second render pass runs once the SignalR circuit connects;
    /// interacting (even just filling a plain HTML form field) between those two passes can have
    /// its value silently wiped by the second render. Waiting for network idle — Playwright's own
    /// documented load-state wait, not a fixed sleep — is a reliable proxy for "the circuit has
    /// connected and settled" since the handshake and initial component round trip are themselves
    /// network activity.
    /// </summary>
    protected async Task GotoAsync(string path)
    {
        await Page.GotoAsync(path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Submits the shared login form without asserting the destination. The calling journey keeps
    /// ownership of its navigation and outcome assertions so failures remain explicit.
    /// </summary>
    protected async Task SubmitLoginAsync(string email, string password)
    {
        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
    }
}
