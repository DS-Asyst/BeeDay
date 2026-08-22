using BeeDay.Web.Components.Features.Institutional.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Institutional;

public sealed class InstitutionalPagesTests
{
    [Fact]
    public void MissionRendersItsHeadingAndBrandContext()
    {
        AssertRendersHeroAndHeading(context => context.Render<Mission>(), "Our mission", "Mission");
    }

    [Fact]
    public void EfficacyRendersItsHeadingAndBrandContext()
    {
        AssertRendersHeroAndHeading(context => context.Render<Efficacy>(), "Efficacy", "Efficacy");
    }

    [Fact]
    public void EfficacyDisclosesNoPublishedEvidenceInsteadOfFabricatingMetricsOrStudies()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Efficacy>());

        Assert.Contains("not yet published", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(cut.Find(".institutional-pending-notice"));
        // No invented percentages or day-streak-style claims anywhere in the page's own copy.
        Assert.DoesNotMatch(@"\d+\s*%", cut.Markup);
    }

    [Fact]
    public void MissionRendersTheFourApprovedStoryBlocksAndTheClosingStatement()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Mission>());

        var blocks = cut.FindAll(".editorial-story-block__headline");
        Assert.Equal(5, blocks.Count);
        Assert.Equal("consistency, not perfection.", blocks[0].TextContent.Trim());
        Assert.Equal("progress you can see.", blocks[1].TextContent.Trim());
        Assert.Equal("everything in one place.", blocks[2].TextContent.Trim());
        Assert.Equal("the control stays with you.", blocks[3].TextContent.Trim());
        Assert.Equal("it isn't about doing it all. it's about continuing to move forward.", blocks[4].TextContent.Trim());
        Assert.Contains("editorial-story-block--centered", cut.FindAll(".editorial-story-block")[4].ClassList);
    }

    // Brand guidelines moved off ExperienceSystemHome to its own Institutional page in Sprint 29.4
    // (see BrandGuidelinesTests) — /experience-system keeps the original ExperienceSystemHome shell.

    [Fact]
    public void ContactRendersRealExistingGitHubAndLinkedInChannelsWithoutFabricatedEmail()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Contact>());

        Assert.Equal("Contact us", cut.Find("h1").TextContent.Trim());
        Assert.NotNull(cut.Find("a[href='https://github.com/tiagoarrigoni/BeeDay']"));
        Assert.NotNull(cut.Find("a[href='https://www.linkedin.com/in/tiago-a-arrigoni-335b9413b/']"));
        Assert.DoesNotContain("mailto:", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductRendersItsHeadingAndThreeRealFeatures()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Product>());

        Assert.Equal("beeday", cut.Find("h1").TextContent.Trim());
        Assert.Equal(3, cut.FindAll(".institutional-product__feature").Count);
        var cta = cut.Find(".beeday-hero__primary-action a.beeday-button--important-white");
        Assert.Equal("/profile/create", cta.GetAttribute("href"));
        Assert.Equal("Get started", cta.TextContent.Trim());
    }

    [Fact]
    public void ProductPlusDisclosesAComingSoonStateWithoutInventedPricing()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ProductPlus>());

        Assert.Equal("beeday Plus", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Coming soon", cut.Find(".institutional-product__status").TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("$", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("/profile/create", cut.Find(".beeday-hero__primary-action a").GetAttribute("href"));
    }

    [Fact]
    public void AndroidDisclosesAComingSoonStateWithoutAnInventedPlayStoreLink()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Android>());

        Assert.Equal("beeday for Android", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Coming soon", cut.Find(".institutional-product__status").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("a[href*='play.google.com']"));
        Assert.Equal("/profile/create", cut.Find(".beeday-hero__primary-action a").GetAttribute("href"));
    }

    [Fact]
    public void IosDisclosesAComingSoonStateWithoutAnInventedAppStoreLink()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Ios>());

        Assert.Equal("beeday for iOS", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Coming soon", cut.Find(".institutional-product__status").TextContent, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll("a[href*='apps.apple.com']"));
        Assert.Equal("/profile/create", cut.Find(".beeday-hero__primary-action a").GetAttribute("href"));
    }

    [Fact]
    public void FaqsRendersFourKeyboardAccessibleAccordionItems()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Faqs>());

        Assert.Equal("beeday FAQs", cut.Find("h1").TextContent.Trim());
        var items = cut.FindAll("details.institutional-faq__item");
        Assert.Equal(4, items.Count);
        Assert.All(items, item => Assert.NotNull(item.QuerySelector("summary")));
        Assert.Contains("What is beeday?", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void FaqsHelpAnswerLinksToTheRealContactPageInsteadOfNamingItAsPlainText()
    {
        // Sprint 27.3's original copy told the reader to "visit our Contact us page" without an
        // actual link — found during the 27.7 audit. It must be a real, working <a>, not text.
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Faqs>());

        var contactLink = cut.Find("a[href='/contact']");
        Assert.Equal("Contact us", contactLink.TextContent.Trim());
        Assert.Contains("beeday-link", contactLink.ClassList);
    }

    [Fact]
    public void CommunityGuidelinesDisclosesAPendingReviewStateInsteadOfFabricatedClauses()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<CommunityGuidelines>());

        Assert.Equal("Community guidelines", cut.Find("h1").TextContent.Trim());
        Assert.Contains("pending review", cut.Find(".institutional-pending-notice").TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(cut.FindAll(".institutional-legal__toc li"));
    }

    [Fact]
    public void TermsDisclosesAPendingReviewStateInsteadOfFabricatedClauses()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Terms>());

        Assert.Equal("Terms of use", cut.Find("h1").TextContent.Trim());
        Assert.Contains("pending review", cut.Find(".institutional-pending-notice").TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(cut.FindAll(".institutional-legal__toc li"));
    }

    [Fact]
    public void PrivacyDisclosesAPendingReviewStateInsteadOfFabricatedClauses()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Privacy>());

        Assert.Equal("Privacy policy", cut.Find("h1").TextContent.Trim());
        Assert.Contains("pending review", cut.Find(".institutional-pending-notice").TextContent, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(cut.FindAll(".institutional-legal__toc li"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_MissionRendersTranslatedHeadingAndContext()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Mission>());

        Assert.Equal("Nossa missão", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Missão", cut.Find(".institutional-hero__context-label").TextContent);
    }

    [Fact]
    public void UnderPortugueseUiCulture_TermsRendersTranslatedHeadingAndPendingReviewNotice()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Terms>());

        Assert.Equal("Termos de uso", cut.Find("h1").TextContent.Trim());
        Assert.Contains("revisão", cut.Find(".institutional-pending-notice").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_FaqsRendersTranslatedQuestions()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Faqs>());

        Assert.Equal("Perguntas frequentes do beeday", cut.Find("h1").TextContent.Trim());
        Assert.Contains("O que é o beeday?", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryRouteUsesAPageHeaderEligibleSurface()
    {
        // Sprint 29.2: page headers are restricted to Cor0/Cor8 — the only two COR0-COR9 tokens whose
        // contrast with white text passes WCAG AA 4.5:1 (docs/brand/03-color-palette.md). Editorial,
        // Help and Product default to Cor0 (brand primary); Legal/document pages default to the more
        // formal Cor8 (dark navy). Protects the 11 routes from drifting back to one of the other 8,
        // previously arbitrary, tokens (Efficacy was Cor3, Contact Cor2, Faqs Cor4, ProductPlus Cor1,
        // Android Cor2, Ios Cor3, CommunityGuidelines Cor9, Terms Cor7 — none of which pass).
        using var context = new BunitContext().WithLocalization();

        void AssertEligible<TComponent>(string expectedSurfaceClass)
            where TComponent : Microsoft.AspNetCore.Components.IComponent
        {
            var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<TComponent>());
            Assert.Contains(expectedSurfaceClass, cut.Find("header.beeday-hero").ClassList);
        }

        AssertEligible<Mission>("beeday-surface-cor0");
        AssertEligible<Efficacy>("beeday-surface-cor0");
        AssertEligible<Contact>("beeday-surface-cor0");
        AssertEligible<Product>("beeday-surface-cor0");
        AssertEligible<ProductPlus>("beeday-surface-cor0");
        AssertEligible<Android>("beeday-surface-cor0");
        AssertEligible<Ios>("beeday-surface-cor0");
        AssertEligible<Faqs>("beeday-surface-cor0");
        AssertEligible<CommunityGuidelines>("beeday-surface-cor8");
        AssertEligible<Terms>("beeday-surface-cor8");
        AssertEligible<Privacy>("beeday-surface-cor8");
        // Sprint 29.4: Brand guidelines is now grouped with Mission/Efficacy/Contact ("About us"),
        // so it now uses their same Cor0 default instead of the Cor8 it used as ExperienceSystemHome.
        AssertEligible<BrandGuidelines>("beeday-surface-cor0");
    }

    [Fact]
    public void NoPageUsesTheRemovedPerFamilyBodyWidthModifierClassesOrWrappingArticle()
    {
        // Sprint 29.2: editorial/help/legal previously narrowed body width to 42-48rem while the
        // hero row stayed at 72rem (product had no matching rule and silently fell back to 72rem
        // already), misaligning the two axes. All four families now share the shell's single
        // .institutional-page__body width, and the hero is no longer nested inside a wrapping
        // <article class="institutional-page"> that accidentally capped its full-bleed background.
        using var context = new BunitContext().WithLocalization();
        var markups = new[]
        {
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Mission>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Efficacy>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Contact>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Product>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ProductPlus>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Android>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Ios>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Faqs>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<CommunityGuidelines>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Terms>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Privacy>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandGuidelines>()).Markup,
        };

        Assert.All(markups, markup =>
        {
            Assert.DoesNotContain("institutional-page__body--editorial", markup, StringComparison.Ordinal);
            Assert.DoesNotContain("institutional-page__body--help", markup, StringComparison.Ordinal);
            Assert.DoesNotContain("institutional-page__body--legal", markup, StringComparison.Ordinal);
            Assert.DoesNotContain("institutional-page__body--product", markup, StringComparison.Ordinal);
            Assert.DoesNotContain("class=\"institutional-page\"", markup, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void NoPageMarkupReferencesDuolingoOrLoremIpsum()
    {
        using var context = new BunitContext().WithLocalization();
        var markups = new[]
        {
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Mission>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Efficacy>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Contact>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Product>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ProductPlus>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Android>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Ios>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Faqs>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<CommunityGuidelines>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Terms>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Privacy>()).Markup,
            BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BrandGuidelines>()).Markup,
        };

        Assert.All(markups, markup =>
        {
            Assert.DoesNotContain("duolingo", markup, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lorem ipsum", markup, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void EveryEditorialRouteDeclaresTheEditorialLayoutNotThePublicLayout()
    {
        // Sprint 29.4 §6/§37/§38: these 12 routes no longer render PublicHeader (white top nav,
        // language-switcher flags, "Continue to beeday") or AppFooter — @layout EditorialLayout
        // replaces @layout PublicLayout for exactly the footer's editorial page families. bUnit
        // renders a page component directly, not through its declared layout, so this is verified by
        // reading each page's own source — the same technique ExperienceSystemHomeTests already uses
        // for its own @layout/@page assertions.
        var pagesRoot = Path.Combine(ResolveRepoRoot(), "src", "BeeDay.Web", "Components", "Features", "Institutional", "Pages");
        var pageFiles = new[]
        {
            "Mission.razor", "Efficacy.razor", "Contact.razor", "Product.razor", "ProductPlus.razor",
            "Android.razor", "Ios.razor", "Faqs.razor", "CommunityGuidelines.razor", "Terms.razor",
            "Privacy.razor", "BrandGuidelines.razor"
        };

        Assert.All(pageFiles, fileName =>
        {
            var source = File.ReadAllText(Path.Combine(pagesRoot, fileName));
            Assert.Contains("@layout BeeDay.Web.Components.Layout.EditorialLayout", source, StringComparison.Ordinal);
            Assert.DoesNotContain("@layout BeeDay.Web.Components.Layout.PublicLayout", source, StringComparison.Ordinal);
        });
    }

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }

    private static void AssertRendersHeroAndHeading<TComponent>(Func<BunitContext, IRenderedComponent<TComponent>> render, string expectedHeading, string expectedPageContext)
        where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => render(context));

        Assert.Single(cut.FindAll("h1"));
        Assert.Equal(expectedHeading, cut.Find("h1").TextContent.Trim());
        Assert.Contains(expectedPageContext, cut.Find(".institutional-hero__context-label").TextContent);
        Assert.NotEmpty(cut.FindAll(".beeday-hero .beeday-brand"));
    }
}
