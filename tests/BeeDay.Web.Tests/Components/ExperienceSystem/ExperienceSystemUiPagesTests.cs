using BeeDay.Web.Components.Features.ExperienceSystem.Pages.Ui;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.ExperienceSystem;

public sealed class ExperienceSystemUiPagesTests
{
    [Fact]
    public void OverviewListsAllFiveUiTopics()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiOverview>());

        Assert.Equal("UI Design System", cut.Find("h1").TextContent.Trim());
        Assert.Equal(
            ["Foundations", "Components", "Product Patterns", "Interaction", "Layout"],
            cut.FindAll(".experience-system-topic-grid__title").Select(el => el.TextContent.Trim()));
    }

    [Fact]
    public void FoundationsPageDeclaresTypographySpacingAndLayerOrder()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiFoundations>());

        Assert.Equal("Foundations", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Coiny", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Nunito", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Navigation", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Toast", cut.Markup, StringComparison.Ordinal);

        Assert.Equal("page", cut.Find(".experience-system-pillar-nav a[href='/experience-system/ui']").GetAttribute("aria-current"));
        Assert.Equal(5, cut.FindAll(".experience-system-topic-nav a").Count);
    }

    [Fact]
    public void PortugueseFoundationsPageLocalizesSpacingSection()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UiFoundations>());

        Assert.Equal("Fundamentos", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Escala de espaçamento", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ComponentsPageListsSharedPrimitivesWithNoV2Governance()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiComponents>());

        Assert.Equal("Components", cut.Find("h1").TextContent.Trim());
        Assert.Contains("26 shared contracts", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("BeeDayButton", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("BeeDaySortable", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseComponentsPageLocalizesGovernanceCallout()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UiComponents>());

        Assert.Equal("Componentes", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Antes de criar algo novo", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductPatternsPageExplainsWalletAndDailySpecializations()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiProductPatterns>());

        Assert.Equal("Product Patterns", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Wallet — a financial pattern", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Habit, Task, To-Do, and Project", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void InteractionPageStatesTheFiveInteractionRules()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiInteraction>());

        Assert.Equal("Interaction", cut.Find("h1").TextContent.Trim());
        Assert.Equal(5, cut.FindAll(".experience-system-page__body ul li").Count);
        Assert.Contains("Hover never substitutes for focus-visible", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseInteractionPageLocalizesMicrointeractionsTable()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UiInteraction>());

        Assert.Equal("Interação", cut.Find("h1").TextContent.Trim());
        Assert.Contains("Hover de card", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void LayoutPageDescribesTheThreeLayoutsAndShellBreakpoint()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<UiLayout>());

        Assert.Equal("Layout", cut.Find("h1").TextContent.Trim());
        Assert.Equal(3, cut.FindAll(".experience-system-table tbody tr").Count);
        Assert.Contains("1200px", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void PortugueseLayoutPageLocalizesShellExplanation()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<UiLayout>());

        Assert.Equal("Layout", cut.Find("h1").TextContent.Trim());
        Assert.Contains("O breakpoint do shell", cut.Markup, StringComparison.Ordinal);
    }
}
