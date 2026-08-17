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
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "How beeday works", Level = 2 })).ToBeVisibleAsync();
        var howSection = Page.Locator(".home-how");
        var howVisual = howSection.Locator(".home-how__visual img");
        await Expect(howVisual).ToBeVisibleAsync();
        Assert.Equal(5, await howSection.Locator(".home-steps > li").CountAsync());
        Assert.Equal(
            ["Define what matters", "Organize your day", "Improve every day", "Track your progress", "Celebrate your wins"],
            await howSection.Locator(".home-steps h3").AllTextContentsAsync());
        var brandClosure = Page.Locator(".home-brand-closure");
        var brandClosureCharacters = brandClosure.Locator(".home-brand-closure__characters");
        var brandClosureWave = brandClosure.Locator(".home-brand-closure__wave");
        var brandClosureBase = brandClosure.Locator(".home-brand-closure__base");
        var topicGroups = brandClosure.Locator(".home-brand-topics > section");
        var footer = Page.GetByRole(AriaRole.Contentinfo);
        await Expect(brandClosureCharacters).ToBeVisibleAsync();
        await Expect(brandClosureWave.Locator("img")).ToBeVisibleAsync();
        await Expect(brandClosure).ToHaveCSSAsync("background-color", "rgb(255, 255, 255)");
        await Expect(brandClosureBase).ToHaveCSSAsync("background-color", "rgb(70, 74, 250)");
        await Expect(topicGroups).ToHaveCountAsync(5);
        var topicItemCounts = await topicGroups.EvaluateAllAsync<int[]>("groups => groups.map(group => group.querySelectorAll('li').length)");
        Assert.Equal(new[] { 2, 2, 2, 2, 2 }, topicItemCounts);
        var topicLinks = brandClosure.Locator(".home-brand-topics a");
        await Expect(topicLinks).ToHaveCountAsync(2);
        await brandClosure.Locator("img").EvaluateAllAsync<object?>(
            "images => Promise.all(images.map(image => image.complete ? Promise.resolve() : new Promise(resolve => image.addEventListener('load', resolve, { once: true }))))");
        await Expect(footer).ToBeVisibleAsync();
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
        var howVisualBox = await Page.Locator(".home-how__visual").BoundingBoxAsync();
        Assert.NotNull(howVisualBox);
        double expectedHowVisualWidth = width > 960
            ? Math.Min(width * .204, 230.4)
            : width > 736
                ? Math.Min(width * .348, 201.6)
                : Math.Min(width * .432, 182.4);
        Assert.InRange(Math.Abs(howVisualBox!.Width - expectedHowVisualWidth), 0, 2);
        var stepBoxes = await howSection.Locator(".home-steps > li").EvaluateAllAsync<double[][]>(
            "elements => elements.map(element => { const box = element.getBoundingClientRect(); return [box.x, box.y, box.width, box.height]; })");
        Assert.All(stepBoxes, stepBox => Assert.False(
            howVisualBox.X < stepBox[0] + stepBox[2]
                && howVisualBox.X + howVisualBox.Width > stepBox[0]
                && howVisualBox.Y < stepBox[1] + stepBox[3]
                && howVisualBox.Y + howVisualBox.Height > stepBox[1],
            "The Bee must not overlap concept copy."));
        if (width > 960)
        {
            Assert.True(stepBoxes[0][0] < howVisualBox.X && stepBoxes[1][0] > howVisualBox.X);
            Assert.True(stepBoxes[2][1] >= howVisualBox.Y + howVisualBox.Height);
            Assert.InRange(Math.Abs((stepBoxes[2][0] + stepBoxes[2][2] / 2) - (howVisualBox.X + howVisualBox.Width / 2)), 0, 2);
            Assert.True(stepBoxes[3][0] < howVisualBox.X && stepBoxes[4][0] > howVisualBox.X);
        }
        var closureBox = await brandClosure.BoundingBoxAsync();
        var closureCharactersBox = await brandClosureCharacters.BoundingBoxAsync();
        var closureWaveBox = await brandClosureWave.BoundingBoxAsync();
        var closureBaseBox = await brandClosureBase.BoundingBoxAsync();
        var footerBox = await footer.BoundingBoxAsync();
        Assert.NotNull(closureBox);
        Assert.NotNull(closureCharactersBox);
        Assert.NotNull(closureWaveBox);
        Assert.NotNull(closureBaseBox);
        Assert.NotNull(footerBox);
        Assert.InRange(Math.Abs(closureBox!.Y + closureBox.Height - footerBox!.Y), 0, 1);
        Assert.InRange(Math.Abs(closureCharactersBox!.Width / closureCharactersBox.Height - 1536d / 1024d), 0, .01);
        Assert.True(closureCharactersBox.Height >= 149, "The characters should remain recognizable on narrow viewports.");
        Assert.InRange(Math.Abs(closureWaveBox!.Y + closureWaveBox.Height - closureBaseBox!.Y), 0, 1);
        Assert.True(closureCharactersBox.X >= 0 && closureCharactersBox.X + closureCharactersBox.Width <= width, "The characters must not be cropped.");
        double expectedCharactersWidth = width > 960
            ? Math.Clamp(width * .216, 268.8, 364.8)
            : width > 736
                ? Math.Clamp(width * .408, 288, 345.6)
                : Math.Min(width * .744, 307.2);
        Assert.InRange(Math.Abs(closureCharactersBox.Width - expectedCharactersWidth), 0, 2);
        Assert.True(closureCharactersBox.Y + closureCharactersBox.Height > closureWaveBox!.Y, "The characters should overlap the wave region instead of floating above it.");
        await topicLinks.First.FocusAsync();
        await Expect(topicLinks.First).ToBeFocusedAsync();
        Assert.Equal("solid", await topicLinks.First.EvaluateAsync<string>("element => getComputedStyle(element).outlineStyle"));
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
        await Expect(Page.Locator("#how-heading")).ToContainTextAsync("Como o beeday funciona");
        await Expect(Page.Locator(".home-brand-topics h2")).ToHaveTextAsync(
            ["Sobre nós", "Social", "Apps", "Ajuda e suporte", "Privacidade e termos"]);
        await Expect(Page.Locator(".home-steps h3")).ToHaveTextAsync(
            ["Defina o que importa", "Organize o seu dia", "Evolua todos os dias", "Acompanhe seu progresso", "Celebre suas conquistas"]);
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
        // EPIC 27 Sprint 27.2: the header lockup grows from the previous bitmap's 46.875px so the
        // wordmark reads as a real brand mark instead of a small, timid icon.
        Assert.Equal("54.4062px", await Page.Locator(".public-header__brand-mark")
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
        // Text-safe accent (not raw COR3, which measures ~2.44:1 on white and fails WCAG AA).
        Assert.Equal("rgb(11, 114, 166)", await existingAccount
            .EvaluateAsync<string>("element => getComputedStyle(element).color"));
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

    [Fact]
    public async Task HowBeeDayWorksBackgroundTracksSectionProgressAndReducedMotion()
    {
        await Page.SetViewportSizeAsync(1280, 800);
        await GotoAsync("/");

        Assert.True(await Page.EvaluateAsync<bool>("() => CSS.supports('animation-timeline: view()')"));

        await ScrollHowSectionToProgressAsync(.1);
        Assert.Equal("rgb(255, 255, 255)", await HowBackgroundAsync());

        await ScrollHowSectionToProgressAsync(.4);
        var enteringColor = await HowBackgroundAsync();
        Assert.NotEqual("rgb(255, 255, 255)", enteringColor);
        Assert.NotEqual("rgb(213, 238, 253)", enteringColor);

        await ScrollHowSectionToProgressAsync(.6);
        Assert.Equal("rgb(213, 238, 253)", await HowBackgroundAsync());

        await ScrollHowSectionToProgressAsync(.85);
        var exitingColor = await HowBackgroundAsync();
        Assert.NotEqual("rgb(255, 255, 255)", exitingColor);
        Assert.NotEqual("rgb(213, 238, 253)", exitingColor);

        await ScrollHowSectionToProgressAsync(.99);
        Assert.Equal("rgb(255, 255, 255)", await HowBackgroundAsync());

        await ScrollHowSectionToProgressAsync(.6);
        Assert.Equal("rgb(213, 238, 253)", await HowBackgroundAsync());
        await ScrollHowSectionToProgressAsync(.4);
        Assert.Equal(enteringColor, await HowBackgroundAsync());

        await ScrollHowSectionToProgressAsync(.6);
        var restoredScrollPosition = await Page.EvaluateAsync<double>("() => window.scrollY");
        await Page.ReloadAsync();
        await Page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
        Assert.InRange(Math.Abs(await Page.EvaluateAsync<double>("() => window.scrollY") - restoredScrollPosition), 0, 2);
        Assert.Equal("rgb(213, 238, 253)", await HowBackgroundAsync());

        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await ScrollHowSectionToProgressAsync(.1);
        Assert.Equal("rgb(213, 238, 253)", await HowBackgroundAsync());
        await Page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.NoPreference });
    }

    private async Task ScrollHowSectionToProgressAsync(double progress)
    {
        await Page.EvaluateAsync(
            "progress => { document.documentElement.style.scrollBehavior = 'auto'; const section = document.querySelector('.home-how'); const rangeStart = section.offsetTop - window.innerHeight; const rangeEnd = section.offsetTop + section.offsetHeight; window.scrollTo(0, rangeStart + (rangeEnd - rangeStart) * progress); }",
            progress);
        await Page.EvaluateAsync("() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)))");
    }

    private async Task<string> HowBackgroundAsync()
    {
        return await Page.Locator(".home-how").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor");
    }

}
