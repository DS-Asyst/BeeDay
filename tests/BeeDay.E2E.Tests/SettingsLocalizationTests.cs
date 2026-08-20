using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Covers the one Sprint 23.3 behavior that only a real browser can prove: changing
/// Settings → Preferences → Language triggers a real JS-interop-driven POST to /culture/set (an
/// interactive Blazor Server component has no live HttpContext to write a Set-Cookie header into
/// directly — see wwwroot/js/beeday-culture-sync.js), the page reloads through it, and the new
/// culture actually takes effect and persists. Precedence rules, cookie attributes, and the
/// login/logout boundary are already proven by AuthenticatedCultureIntegrationTests at the HTTP
/// level — this test does not repeat that, only the part integration tests structurally cannot
/// reach.
/// </summary>
public sealed class SettingsLocalizationTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Fact]
    public async Task ChangingLanguageInSettings_ReloadsInPortugueseAndPersistsOnReturn()
    {
        var email = $"e2e-settings-language-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await SubmitLoginAsync(email, Password);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await GotoAsync("/account");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "My Account" })).ToBeVisibleAsync();

        await Page.GetByLabel("Language").SelectOptionAsync(new SelectOptionValue { Label = "Portuguese" });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save Preferences" }).ClickAsync();

        // The save triggers a real full-page POST to /culture/set and a redirect back — a genuine
        // browser navigation, not an interactive Blazor update. Asserting on the Portuguese
        // heading (a Playwright web-first assertion, auto-retrying) is what actually waits for
        // that navigation to finish; the URL itself never changes, since it redirects back to the
        // same /account path.
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Minha Conta" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Salvar Preferências" })).ToBeVisibleAsync();

        // Revisiting later (a fresh navigation) still shows Portuguese selected — proves the
        // account itself, not just a one-off cookie, now carries the choice.
        await GotoAsync("/account");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Minha Conta" })).ToBeVisibleAsync();
        await Expect(Page.GetByLabel("Idioma")).ToHaveValueAsync(nameof(BeeDay.Domain.Enums.UserLanguage.Portuguese));
    }
}
