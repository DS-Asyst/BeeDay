using System.Text.RegularExpressions;
using BeeDay.Web.Tests.Components.Layout;
using BeeDay.Web.Tests.Localization;
using HomePage = BeeDay.Web.Components.Features.Home.Pages.Home;

namespace BeeDay.Web.Tests.Components.Home;

public sealed class HomeTests
{
    [Fact]
    public void RendersSingleH1WithProductPromise()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HomePage>());

        var headings = cut.FindAll("h1");
        Assert.Single(headings);
        Assert.Contains("one step at a time", headings[0].TextContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AnonymousCallsToActionTargetRegistrationAndLogin()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HomePage>());

        Assert.Single(cut.FindAll("a[href='/profile/create']"));
        Assert.NotNull(cut.Find(".home-hero__login[href='/login']"));
        Assert.Contains("beeday-button--secondary", cut.Find(".home-hero__login").ClassList);
    }

    [Fact]
    public void PresentsRealProductConceptsAndSimpleProcess()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HomePage>());

        Assert.Equal(["Define", "Practice", "Evolve"], cut.FindAll(".home-steps h3").Select(element => element.TextContent.Trim()));
        var heroImage = cut.Find(".home-hero__visual img.home-hero__image");
        Assert.Equal("/assets/hero/home-team.png", heroImage.GetAttribute("src"));
        var howImage = cut.Find(".home-how__visual img");
        Assert.Equal("/assets/home/how-beeday-works-bee.png", howImage.GetAttribute("src"));
        Assert.Equal(string.Empty, howImage.GetAttribute("alt"));
        Assert.Equal("lazy", howImage.GetAttribute("loading"));
        var brandClosure = cut.Find(".home-brand-closure");
        var brandClosureImage = brandClosure.QuerySelector("img");
        Assert.NotNull(brandClosureImage);
        Assert.Equal("/assets/home/home-team-fall-color.png", brandClosureImage.GetAttribute("src"));
        Assert.Equal(string.Empty, brandClosureImage.GetAttribute("alt"));
        Assert.Equal("lazy", brandClosureImage.GetAttribute("loading"));
        Assert.Contains("home-brand-closure", cut.Find(".home-page").LastElementChild!.ClassList);
        Assert.Empty(cut.FindAll(".home-hero .beeday-brand"));
        Assert.Empty(cut.FindAll(".home-hero__symbol"));
        Assert.Empty(cut.FindAll(".home-preview, .home-values, .home-growth, .home-cta"));
    }

    [Fact]
    public void DoesNotPresentUnsupportedGamificationOrFabricatedMetrics()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HomePage>());

        Assert.False(Regex.IsMatch(cut.Markup, @"\d+\s*(%|day|days)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        Assert.DoesNotContain("streak", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("achievement", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("XP today", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersThePortugueseHomeResources()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<HomePage>());

        Assert.Contains("Construa um dia melhor", cut.Find("h1").TextContent, StringComparison.Ordinal);
        Assert.Contains("Como o BeeDay funciona", cut.Find("h2").TextContent, StringComparison.Ordinal);
        Assert.Contains("Comece agora", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Já tenho uma conta", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Build a better day", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Get started", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AuthenticatedVisitorSeesContinueCta()
    {
        using var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.AddAuthorization().SetAuthorized("test-user");
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HomePage>());

        Assert.Contains("Continue to BeeDay", cut.Markup, StringComparison.Ordinal);
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.AddAuthorization().SetNotAuthorized();
        PublicHeaderTests.RegisterDestinationResolver(context, hasProfile: true, hasCompletedOnboarding: true);
        return context;
    }
}
