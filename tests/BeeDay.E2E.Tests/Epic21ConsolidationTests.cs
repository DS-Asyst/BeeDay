using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class Epic21ConsolidationTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    private const string Password = "E2ePassword123!";

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1440, 900)]
    [InlineData(1280, 800)]
    [InlineData(1024, 800)]
    [InlineData(900, 800)]
    [InlineData(768, 900)]
    [InlineData(430, 860)]
    [InlineData(390, 844)]
    public async Task FinalExperienceMatrixHasNoDocumentOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);

        foreach (var publicRoute in new[] { "/", "/login" })
        {
            await GotoAsync(publicRoute);
            await Expect(Page.Locator("main").First).ToBeVisibleAsync();
            await AssertNoDocumentOverflowAsync(publicRoute, width);
        }

        var email = $"e2e-epic21-{Guid.NewGuid():N}@beeday.invalid";
        await Fixture.Factory.SeedUserAsync(email, Password, onboardingCompleted: true);
        await SubmitLoginAsync(email, Password);
        await Expect(Page).ToHaveURLAsync(new Regex("/profile$"));
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var authenticatedRoute in new[] { "/profile", "/daily", "/wallet", "/settings" })
        {
            await GotoAsync(authenticatedRoute);
            await Expect(Page.Locator("main").First).ToBeVisibleAsync();
            await AssertNoDocumentOverflowAsync(authenticatedRoute, width);
        }
    }

    private async Task AssertNoDocumentOverflowAsync(string route, int width)
    {
        var hasOverflow = await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasOverflow, $"{route} has document overflow at {width}px.");
    }
}
