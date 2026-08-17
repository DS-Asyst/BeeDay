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

    // Brand guidelines moved to ExperienceSystemHome (a second @page route on the same component
    // that already powers /experience-system) in Sprint 27.8 — see ExperienceSystemHomeTests for
    // its coverage. The Institutional feature no longer owns this route.

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
        };

        Assert.All(markups, markup =>
        {
            Assert.DoesNotContain("duolingo", markup, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("lorem ipsum", markup, StringComparison.OrdinalIgnoreCase);
        });
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
