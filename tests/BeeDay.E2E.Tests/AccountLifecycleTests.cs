using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Account journeys driven entirely through the real browser: registration, login (which also
/// exercises onboarding completion, since a freshly registered/seeded user cannot reach the
/// dashboard any other way), and logout. Antiforgery, cookies, SessionVersion, and internal
/// authorization are Sprint 12.6's job (real HTTP-pipeline integration tests) — these tests only
/// assert what a real user sees in the browser.
/// </summary>
public sealed class AccountLifecycleTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task CreateAccount_ReachesEmailConfirmationPending()
    {
        var email = $"e2e-create-{Guid.NewGuid():N}@beeday.invalid";

        await GotoAsync("/login");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Create account" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/profile/create$"));
        await Page.GetByLabel("Full name").FillAsync("E2E New User");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync(Password);
        await Page.GetByLabel("Confirm password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Continue" }).ClickAsync();

        await Page.GetByLabel("Nickname").FillAsync($"e2e{Guid.NewGuid():N}"[..12]);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Create account" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/account/email-confirmation-sent"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Check your email" })).ToBeVisibleAsync();
        await Expect(Page.GetByText(email)).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Login_CompletesOnboarding_ReachesDashboard()
    {
        var email = $"e2e-login-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: false);

        await LoginAsync(email);

        await Expect(Page).ToHaveURLAsync(new Regex("/onboarding/tutorial$"));

        // Each slide advance is a Blazor interactive click (not a form post); the very first one can
        // silently no-op if it lands before the SignalR circuit has connected. Looping on the
        // "ENTER BEEDAY" button's own visibility (a real, render-confirmed state — the button's text
        // only becomes "ENTER BEEDAY" on the last slide) — rather than assuming a fixed count of 4
        // clicks — absorbs that without asserting on internal slide state or using a fixed sleep.
        var enterBeeDay = Page.GetByRole(AriaRole.Button, new() { Name = "ENTER BEEDAY" });
        for (var attempt = 0; attempt < 8 && !await enterBeeDay.IsVisibleAsync(); attempt++)
        {
            await Page.GetByRole(AriaRole.Button, new() { Name = "NEXT" }).ClickAsync();
            try
            {
                await enterBeeDay.WaitForAsync(new LocatorWaitForOptions { Timeout = 800 });
            }
            catch (TimeoutException)
            {
            }
        }

        await enterBeeDay.ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("Welcome back") })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Logout_EndsSessionAndBlocksDashboard()
    {
        var email = $"e2e-logout-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await LoginAsync(email);
        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));

        await Page.GetByRole(AriaRole.Button, new() { Name = "Log out of BeeDay" }).ClickAsync();

        await Expect(Page).ToHaveURLAsync(new Regex("/login"));

        await GotoAsync("/daily");
        await Expect(Page).ToHaveURLAsync(new Regex("/login"));
    }

    [Fact]
    public async Task EditProfile_UpdatesName()
    {
        var email = $"e2e-profile-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await LoginAsync(email);
        await Expect(Page).ToHaveURLAsync(new Regex("/home$"));

        await GotoAsync("/account");
        var updatedName = $"E2E Updated {Guid.NewGuid():N}"[..20];
        // The "Name" field is marked required, and the required-marker asterisk turns out to be
        // part of the computed accessible label ("Name*", confirmed via the failure screenshot
        // this test's own infrastructure captured) rather than excluded via aria-hidden as the
        // source seemed to imply — matching loosely for the optional marker avoids relying on that.
        await Page.GetByLabel(new Regex("^Name\\*?$")).FillAsync(updatedName);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Profile" }).ClickAsync();

        await Expect(Page.GetByText("Profile saved")).ToBeVisibleAsync();
    }

    private async Task LoginAsync(string email)
    {
        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

        // The Sign In click triggers a real server-side redirect to a brand new page (/daily or
        // /onboarding/tutorial), which establishes its own SignalR circuit; GotoAsync's network-idle
        // wait only covers explicit navigations, not ones reached via a redirect, so it has to be
        // repeated here before any caller performs an interactive (non-form-post) click.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
