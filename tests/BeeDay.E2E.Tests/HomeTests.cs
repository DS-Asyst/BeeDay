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
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Log in" })).ToHaveCountAsync(0);
        await Expect(Page.Locator(".public-header").GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".public-header").GetByRole(AriaRole.Button, new() { Name = "English (United States)" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToBeVisibleAsync();
        await Expect(Page.Locator(".public-header").GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToHaveCountAsync(0);

        await Page.GetByRole(AriaRole.Link, new() { Name = "I already have an account" }).ClickAsync();
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

        var heroVisual = Page.Locator(".home-hero__visual");
        var heroImage = heroVisual.Locator("img.home-hero__image");
        await Expect(heroImage).ToBeVisibleAsync();
        await Expect(Page.Locator(".home-hero .beeday-brand")).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "How BeeDay works", Level = 2 })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Contentinfo)).ToBeVisibleAsync();
        var visualBox = await heroVisual.BoundingBoxAsync();
        var contentBox = await Page.Locator(".home-hero__content").BoundingBoxAsync();
        Assert.NotNull(visualBox);
        Assert.NotNull(contentBox);
        Assert.InRange(Math.Abs(visualBox!.Width - visualBox.Height), 0, 1);
        if (width >= 1024)
        {
            Assert.True(visualBox.X < contentBox!.X, "Desktop Hero should place the illustration to the left of its content.");
            Assert.InRange(Math.Abs(visualBox.Y - contentBox.Y), 0, visualBox.Height * .35);
        }
        else
        {
            Assert.True(visualBox.Y < contentBox!.Y, "Tablet/mobile Hero should stack the illustration above its content.");
        }
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task PublicHeaderActionsHaveVisibleKeyboardFocus()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        var portuguese = Page.GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" });
        await portuguese.FocusAsync();
        await Expect(portuguese).ToBeFocusedAsync();
        Assert.Equal("solid", await portuguese.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));

        await Page.Locator(".public-header__brand").FocusAsync();
        await Expect(Page.Locator(".public-header__brand")).ToBeFocusedAsync();
        Assert.Equal("solid", await Page.Locator(".public-header__brand").EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
    }

    [Fact]
    public async Task SelectingLanguageTranslatesPublicHomeContentAndTracksActiveState()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        var portuguese = Page.GetByRole(AriaRole.Button, new() { Name = "Português (Brasil)" });
        var english = Page.GetByRole(AriaRole.Button, new() { Name = "English (United States)" });

        await Expect(english).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(portuguese).ToHaveAttributeAsync("aria-pressed", "false");

        await portuguese.ClickAsync();

        await Expect(portuguese).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(english).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Page).ToHaveURLAsync(new Regex(@"/$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToContainTextAsync("Construa um dia melhor");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 2 })).ToContainTextAsync("Como o BeeDay funciona");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Comece agora" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Já tenho uma conta" })).ToBeVisibleAsync();

        await english.ClickAsync();

        await Expect(english).ToHaveAttributeAsync("aria-pressed", "true");
        await Expect(portuguese).ToHaveAttributeAsync("aria-pressed", "false");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Level = 1 })).ToContainTextAsync("Build a better day");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToBeVisibleAsync();
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
        Assert.Equal("46.875px", await Page.Locator(".public-header__brand-mark")
            .EvaluateAsync<string>("element => getComputedStyle(element).height"));

        var heroActions = Page.Locator(".home-hero__actions");
        var getStarted = heroActions.GetByRole(AriaRole.Link, new() { Name = "Get started" });
        var existingAccount = heroActions.GetByRole(AriaRole.Link, new() { Name = "I already have an account" });
        Assert.Equal("rgb(82, 71, 249)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        Assert.Equal("rgb(28, 14, 242)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).borderBottomColor"));
        Assert.Equal("rgb(255, 255, 255)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).color"));
        Assert.Equal("rgb(255, 255, 255)", await existingAccount
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        await existingAccount.FocusAsync();
        Assert.Equal("solid", await existingAccount.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
        await getStarted.HoverAsync();
        await Expect(getStarted).ToHaveCSSAsync("background-color", "rgb(63, 51, 241)");
        await Page.Mouse.DownAsync();
        await Expect(getStarted).ToHaveCSSAsync("background-color", "rgb(28, 14, 242)");
        await Page.Mouse.MoveAsync(0, 0);
        await Page.Mouse.UpAsync();
        Assert.Equal("rgb(247, 247, 247)", await Page.GetByRole(AriaRole.Contentinfo)
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }
}
