using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class AppFooterTests
{
    [Fact]
    public void RendersTheInverseBrandLockupAndTagline()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        Assert.Contains("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
        var brand = cut.Find(".app-footer__identity .beeday-brand");
        Assert.Contains("app-footer__brand-mark", brand.ClassList);
        Assert.Contains("beeday-brand--inverse", brand.ClassList);
    }

    [Fact]
    public void RendersExactlySixGroupsWithTheDocumentedLinksToInternalRoutes()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        var groups = cut.FindAll(".app-footer__group");
        Assert.Equal(6, groups.Count);
        Assert.Equal(
            ["About us", "Products", "Apps", "Help and support", "Privacy and terms", "Social"],
            groups.Select(group => group.QuerySelector("h2")!.TextContent.Trim()));

        var aboutUs = groups[0];
        Assert.Equal(
            ["/mission", "/efficacy", "/brand-guidelines", "/contact"],
            aboutUs.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));

        var products = groups[1];
        Assert.Equal(["/beeday", "/beeday-plus"], products.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));

        var apps = groups[2];
        Assert.Equal(["/android", "/ios"], apps.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));

        var help = groups[3];
        Assert.Equal(["/faqs"], help.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));

        var legal = groups[4];
        Assert.Equal(
            ["/community-guidelines", "/terms", "/privacy"],
            legal.QuerySelectorAll("a").Select(a => a.GetAttribute("href")));
    }

    [Fact]
    public void SocialGroupHasARealLinkedInLinkAndNonInteractivePlaceholdersForUnavailableChannels()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        var social = cut.Find(".app-footer__group--social");
        var linkedIn = social.QuerySelector("a[href='https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/']");
        Assert.NotNull(linkedIn);
        Assert.Equal("LinkedIn", linkedIn.GetAttribute("aria-label"));

        // No real Instagram/X URL exists in the repository — rendered as a non-clickable placeholder
        // instead of a link to "#" or an invented handle.
        Assert.Single(social.QuerySelectorAll("a"));
        Assert.Equal(2, social.QuerySelectorAll(".app-footer__social-unavailable").Length);
    }

    [Fact]
    public void DoesNotRenderTheRemovedGitHubOrStandaloneExperienceSystemEntries()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        Assert.Empty(cut.FindAll("a[href='https://github.com/tiagoarrigoni/BeeDay']"));
        Assert.Empty(cut.FindAll("a[href='/experience-system']"));
        Assert.Empty(cut.FindAll("a[href='#']"));
        Assert.All(cut.FindAll("a"), link =>
        {
            var href = link.GetAttribute("href")!;
            Assert.True(href.StartsWith('/') || href.StartsWith("https://", StringComparison.Ordinal), $"Unexpected href: {href}");
        });
    }

    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishGroupTitlesAndCopyright()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<AppFooter>());

        Assert.Equal("Brand guidelines", cut.Find("a[href='/brand-guidelines']").TextContent);
        Assert.Equal("© 2026 beeday. All rights reserved.", cut.Find(".app-footer__copyright").TextContent);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseGroupTitlesAndCopyright()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<AppFooter>());

        Assert.Equal(
            ["Sobre nós", "Produtos", "Aplicativos", "Ajuda e suporte", "Privacidade e termos", "Social"],
            cut.FindAll(".app-footer__group h2").Select(h => h.TextContent.Trim()));
        Assert.Equal("Fale conosco", cut.Find("a[href='/contact']").TextContent);
        Assert.Equal("© 2026 beeday. Todos os direitos reservados.", cut.Find(".app-footer__copyright").TextContent);
    }

    [Fact]
    public void Tagline_UnderPortugueseUiCulture_RendersThePortugueseResource()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<AppFooter>());

        Assert.Contains("Seja melhor a cada dia", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Be Better Every Day", cut.Markup, StringComparison.Ordinal);
    }
}
