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

        Assert.Equal(
            ["Define what matters", "Organize your day", "Improve every day", "Track your progress", "Celebrate your wins"],
            cut.FindAll(".home-steps h3").Select(element => element.TextContent.Trim()));
        Assert.Equal("How beeday works", cut.Find("#how-heading").TextContent.Trim());
        Assert.Equal(["1", "2", "3", "4", "5"], cut.FindAll(".home-steps > li > span").Select(element => element.TextContent.Trim()));
        var heroImage = cut.Find(".home-hero__visual img.home-hero__image");
        Assert.Equal("/assets/hero/home-team.png", heroImage.GetAttribute("src"));
        var howImage = cut.Find(".home-how__visual img");
        Assert.Equal("/assets/home/how-beeday-works-bee.png", howImage.GetAttribute("src"));
        Assert.Equal(string.Empty, howImage.GetAttribute("alt"));
        Assert.Equal("lazy", howImage.GetAttribute("loading"));
        var brandClosure = cut.Find(".home-brand-closure");
        var brandClosureImages = brandClosure.QuerySelectorAll("img");
        Assert.Equal(2, brandClosureImages.Length);
        Assert.Equal("/assets/home/home-team-fall.png", brandClosureImages[0].GetAttribute("src"));
        Assert.Equal("/assets/home/wave-site.png", brandClosureImages[1].GetAttribute("src"));
        Assert.All(brandClosureImages, image =>
        {
            Assert.Equal(string.Empty, image.GetAttribute("alt"));
            Assert.Equal("lazy", image.GetAttribute("loading"));
            Assert.Equal("async", image.GetAttribute("decoding"));
        });
        Assert.DoesNotContain("home-team-fall-color.png", brandClosure.InnerHtml, StringComparison.Ordinal);
        Assert.NotNull(brandClosure.QuerySelector(".home-brand-closure__base"));
        var topicGroups = brandClosure.QuerySelectorAll(".home-brand-topics > section");
        Assert.Equal(5, topicGroups.Length);
        Assert.All(topicGroups, group => Assert.Equal(2, group.QuerySelectorAll("li").Length));
        Assert.Equal(
            ["About us", "Social", "Apps", "Help and support", "Privacy and terms"],
            topicGroups.Select(group => group.QuerySelector("h2")!.TextContent.Trim()));
        Assert.Equal(2, brandClosure.QuerySelectorAll(".home-brand-topics a").Length);
        Assert.NotNull(brandClosure.QuerySelector("a[href='https://github.com/tiagoarrigoni/BeeDay']"));
        Assert.NotNull(brandClosure.QuerySelector("a[href='https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/']"));
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
        Assert.Contains("Como o beeday funciona", cut.Find("h2").TextContent, StringComparison.Ordinal);
        Assert.Equal(
            ["Defina o que importa", "Organize o seu dia", "Evolua todos os dias", "Acompanhe seu progresso", "Celebre suas conquistas"],
            cut.FindAll(".home-steps h3").Select(element => element.TextContent.Trim()));
        Assert.Contains("Transforme seus objetivos em prioridades claras e saiba onde concentrar sua energia.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Reúna sua rotina, tarefas e compromissos em um só lugar para viver com mais clareza.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Pequenas ações consistentes criam grandes mudanças. Avance um pouco a cada dia.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Veja sua evolução, mantenha a motivação e entenda o quanto você já avançou.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Cada passo conta. Reconheça suas vitórias e continue evoluindo.", cut.Markup, StringComparison.Ordinal);
        Assert.Equal(
            ["Sobre nós", "Social", "Apps", "Ajuda e suporte", "Privacidade e termos"],
            cut.FindAll(".home-brand-topics h2").Select(element => element.TextContent.Trim()));
        Assert.Contains("Nossa missão", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Política de privacidade", cut.Markup, StringComparison.Ordinal);
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

        Assert.Contains("Continue to beeday", cut.Markup, StringComparison.Ordinal);
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
