using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace BeeDay.E2E.Tests;

public sealed class HomeTests(PlaywrightAppFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task ReportsRiveAssetMetadataFromTheLoadedRuntime()
    {
        await GotoAsync("/");
        await Expect(Page.Locator(".home-hero__visual")).ToHaveAttributeAsync("data-rive-state", "ready");

        var json = await Page.EvaluateAsync<string>("""
            async () => {
                const module = await import('/js/public-home-rive.js');
                return JSON.stringify(module.inspect(document.querySelector('.home-hero__rive')));
            }
            """);

        using var diagnostics = JsonDocument.Parse(json);
        var root = diagnostics.RootElement;

        Assert.Contains("Blink", root.GetProperty("animationNames").EnumerateArray().Select(value => value.GetString()));
        Assert.Contains("Breathe", root.GetProperty("animationNames").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["State Machine 1"], root.GetProperty("stateMachineNames").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["State Machine 1"], root.GetProperty("playingStateMachineNames").EnumerateArray().Select(value => value.GetString()));
        Assert.Empty(root.GetProperty("playingAnimationNames").EnumerateArray());
        Assert.Equal(
            ["Bubble gum", "eye squint", "hair 4 overlay", "hair 2.5 overlay", "hair 3 overlay", "hair 2 overlay", "hair1 overlay"],
            root.GetProperty("stateMachineInputs").GetProperty("State Machine 1").EnumerateArray()
                .Select(value => value.GetProperty("name").GetString()));
        Assert.True(root.GetProperty("isPlaying").GetBoolean());
        Assert.False(root.GetProperty("isPaused").GetBoolean());
    }

    [Theory]
    [InlineData(1280, 800)]
    [InlineData(390, 844)]
    public async Task RiveStateMachineContinuouslyUpdatesCanvasAfterHydration(int width, int height)
    {
        await Page.SetViewportSizeAsync(width, height);
        await GotoAsync("/");

        var host = Page.Locator(".home-hero__visual");
        var canvas = host.Locator("canvas");
        await Expect(host).ToHaveAttributeAsync("data-rive-state", "ready");
        await Expect(host).ToHaveAttributeAsync("data-rive-motion", "playing");

        var firstFrame = await canvas.EvaluateAsync<string>("element => element.toDataURL()");
        await Task.Delay(350, TestContext.Current.CancellationToken);
        var secondFrame = await canvas.EvaluateAsync<string>("element => element.toDataURL()");
        await Task.Delay(650, TestContext.Current.CancellationToken);
        var thirdFrame = await canvas.EvaluateAsync<string>("element => element.toDataURL()");

        Assert.NotEqual(firstFrame, secondFrame);
        Assert.NotEqual(secondFrame, thirdFrame);
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

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

        var riveHost = Page.Locator(".home-hero__visual");
        await Expect(riveHost).ToHaveAttributeAsync("data-rive-state", "ready");
        await Expect(riveHost.Locator("canvas")).ToBeVisibleAsync();
        await Expect(Page.Locator(".home-hero .beeday-brand")).ToHaveCountAsync(0);
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "How BeeDay works", Level = 2 })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Contentinfo)).ToBeVisibleAsync();
        var visualBox = await riveHost.BoundingBoxAsync();
        var contentBox = await Page.Locator(".home-hero__content").BoundingBoxAsync();
        Assert.NotNull(visualBox);
        Assert.NotNull(contentBox);
        if (width >= 1024)
        {
            Assert.True(visualBox!.X < contentBox!.X, "Desktop Hero should place Rive to the left of its content.");
            Assert.InRange(Math.Abs(visualBox.Y - contentBox.Y), 0, visualBox.Height * .35);
        }
        else
        {
            Assert.True(visualBox!.Y < contentBox!.Y, "Tablet/mobile Hero should stack Rive above its content.");
        }
        Assert.False(await Page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    [Fact]
    public async Task PublicHomeRiveHonorsReducedMotionWithoutConsoleErrors()
    {
        var consoleErrors = new List<string>();
        EventHandler<IConsoleMessage> consoleHandler = (_, message) =>
        {
            if (message.Type == "error")
            {
                consoleErrors.Add(message.Text);
            }
        };

        Page.Console += consoleHandler;
        try
        {
            await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
            await GotoAsync("/");
            await Expect(Page.Locator(".home-hero__visual")).ToHaveAttributeAsync("data-rive-state", "ready");
            await Expect(Page.Locator(".home-hero__visual")).ToHaveAttributeAsync("data-rive-motion", "paused");
            Assert.Empty(consoleErrors);
        }
        finally
        {
            Page.Console -= consoleHandler;
            await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.NoPreference });
        }
    }

    [Fact]
    public async Task PublicHomeRemainsUsableWhenRiveAssetFails()
    {
        await Page.RouteAsync("**/public-home-hero.riv", route => route.AbortAsync());
        try
        {
            await GotoAsync("/");
            await Expect(Page.Locator(".home-hero__visual")).ToHaveAttributeAsync("data-rive-state", "error");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex("one step at a time", RegexOptions.IgnoreCase) })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Get started" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "I already have an account" })).ToBeVisibleAsync();
        }
        finally
        {
            await Page.UnrouteAsync("**/public-home-hero.riv");
        }
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
        Assert.Equal("36px", await Page.Locator(".public-header .beeday-brand")
            .EvaluateAsync<string>("element => getComputedStyle(element).fontSize"));

        var heroActions = Page.Locator(".home-hero__actions");
        var getStarted = heroActions.GetByRole(AriaRole.Link, new() { Name = "Get started" });
        var existingAccount = heroActions.GetByRole(AriaRole.Link, new() { Name = "I already have an account" });
        Assert.Equal("rgb(0, 121, 185)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        Assert.Equal("rgb(0, 109, 168)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).borderBottomColor"));
        Assert.Equal("rgb(255, 255, 255)", await getStarted
            .EvaluateAsync<string>("element => getComputedStyle(element).color"));
        Assert.Equal("rgb(255, 255, 255)", await existingAccount
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
        await existingAccount.FocusAsync();
        Assert.Equal("solid", await existingAccount.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
        await getStarted.HoverAsync();
        await Expect(getStarted).ToHaveCSSAsync("background-color", "rgb(0, 124, 189)");
        await Page.Mouse.DownAsync();
        await Expect(getStarted).ToHaveCSSAsync("background-color", "rgb(0, 109, 168)");
        await Page.Mouse.MoveAsync(0, 0);
        await Page.Mouse.UpAsync();
        Assert.Equal("rgb(247, 247, 247)", await Page.GetByRole(AriaRole.Contentinfo)
            .EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
    }
}
