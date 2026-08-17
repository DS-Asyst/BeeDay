using System.Text.RegularExpressions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Representative automated accessibility checks. A clean axe result prevents detectable markup,
/// name, role, structure, and contrast regressions; it is not a claim of complete WCAG compliance.
/// </summary>
public sealed class AccessibilityQualityTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Theory]
    [InlineData("/")]
    [InlineData("/brand/typography")]
    [InlineData("/experience-system")]
    [InlineData("/login")]
    [InlineData("/mission")]
    [InlineData("/efficacy")]
    [InlineData("/brand-guidelines")]
    [InlineData("/contact")]
    [InlineData("/beeday")]
    [InlineData("/beeday-plus")]
    [InlineData("/android")]
    [InlineData("/ios")]
    [InlineData("/faqs")]
    [InlineData("/community-guidelines")]
    [InlineData("/terms")]
    [InlineData("/privacy")]
    public async Task PublicHighValuePages_HaveNoAutomaticallyDetectableViolations(string route)
    {
        await Page.SetViewportSizeAsync(1280, 900);
        await GotoAsync(route);

        await AssertAxeCleanAsync(Page, route);
    }

    [Fact]
    public async Task DailyWalletAndCanonicalDialog_HaveNoAutomaticallyDetectableViolations()
    {
        await Page.SetViewportSizeAsync(1280, 900);
        await LoginAsync();

        await GotoAsync("/daily");
        await Expect(Page.Locator(".dashboard-page")).ToBeVisibleAsync();
        await AssertAxeCleanAsync(Page, "/daily");

        await GotoAsync("/wallet");
        await Expect(Page.Locator(".wallet-page")).ToBeVisibleAsync();
        await AssertAxeCleanAsync(Page, "/wallet");

        await Page.GetByRole(AriaRole.Button, new() { Name = "New transaction" }).ClickAsync();
        var dialog = Page.GetByRole(AriaRole.Dialog);
        await Expect(dialog).ToBeVisibleAsync();
        await AssertAxeCleanAsync(dialog, "Wallet transaction dialog");
    }

    private async Task LoginAsync()
    {
        var email = $"e2e-axe-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);

        await GotoAsync("/login");
        await Page.GetByLabel("Email").FillAsync(email);
        await Page.GetByLabel("Password").FillAsync(Password);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task AssertAxeCleanAsync(IPage page, string context)
    {
        var result = await page.RunAxe();
        AssertNoViolations(result, context);
    }

    private static async Task AssertAxeCleanAsync(ILocator locator, string context)
    {
        var result = await locator.RunAxe();
        AssertNoViolations(result, context);
    }

    private static void AssertNoViolations(AxeResult result, string context)
    {
        var details = string.Join(
            Environment.NewLine,
            result.Violations.Select(violation =>
                $"{violation.Id} ({violation.Impact}): {string.Join(" | ", violation.Nodes.Select(node => node.Html))}"));

        Assert.False(result.Violations.Any(), $"axe found violations in {context}:{Environment.NewLine}{details}");
    }
}
