using System.Text.RegularExpressions;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class HomeTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task AnonymousVisitorSeesOfficialHomeAndCanReachAuthenticationFlows()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        await Expect(Page).ToHaveURLAsync(new Regex(@"/$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("one step at a time", RegexOptions.IgnoreCase), Level = 1 })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Log in" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".public-header").GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToHaveCountAsync(0);

        await Page.GetByRole(AriaRole.Link, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/login$"));
        await Page.GetByRole(AriaRole.Link, new() { Name = "Close login and return to Home" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"/$"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Get started" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex("/profile/create$"));
    }

    [Theory]
    [InlineData(390, 844)]
    [InlineData(430, 860)]
    [InlineData(768, 900)]
    [InlineData(1024, 800)]
    [InlineData(1280, 800)]
    [InlineData(1440, 900)]
    [InlineData(1920, 1080)]
    public async Task PublicHomeIsResponsiveAccessibleAndDoesNotOverflow(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/");

        await Expect(Page.Locator(".home-hero__brand .beeday-brand")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "How BeeDay works", Level = 2 })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Contentinfo)).ToBeVisibleAsync();
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task PublicHeaderActionsHaveVisibleKeyboardFocus()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        var login = Page.GetByRole(AriaRole.Link, new() { Name = "Log in" });
        await login.FocusAsync();
        await Expect(login).ToBeFocusedAsync();
        Assert.Equal("solid", await login.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));

        await Page.Locator(".public-header__brand").FocusAsync();
        await Expect(Page.Locator(".public-header__brand")).ToBeFocusedAsync();
        Assert.Equal("solid", await Page.Locator(".public-header__brand").EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
    }

    [Fact]
    public async Task HeaderHeroColorsAndFooterFollowPublicVisualHierarchy()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        var headerBox = await Page.Locator(".public-header").BoundingBoxAsync();
        var heroBox = await Page.Locator(".home-hero").BoundingBoxAsync();
        Assert.NotNull(headerBox);
        Assert.NotNull(heroBox);
        Assert.InRange(Math.Abs((headerBox!.Y + headerBox.Height) - heroBox!.Y), 0, 1);

        var heroActions = Page.Locator(".home-hero__actions");
        var getStarted = heroActions.GetByRole(AriaRole.Link, new() { Name = "Get started" });
        var existingAccount = heroActions.GetByRole(AriaRole.Link, new() { Name = "I already have an account" });
        Assert.Equal("rgb(20, 173, 255)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        Assert.Equal("rgb(7, 152, 226)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).borderBottomColor"));
        await getStarted.HoverAsync();
        await Expect(getStarted).ToHaveCSSAsync("background-color", "rgb(44, 186, 255)");
        Assert.Equal("rgb(255, 255, 255)", await existingAccount
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        await existingAccount.FocusAsync();
        Assert.Equal("solid", await existingAccount.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
        Assert.Equal("rgb(247, 247, 247)", await Page.GetByRole(AriaRole.Contentinfo)
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }
}
