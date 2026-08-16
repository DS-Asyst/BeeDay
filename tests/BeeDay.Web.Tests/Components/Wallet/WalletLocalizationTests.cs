using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Wallet;

/// <summary>
/// Sprint 23.6 coverage that doesn't fit naturally into the existing structural test files:
/// the Wallet-owned skeleton aria-label, Portuguese rendering of the components already covered
/// in English elsewhere, and the transaction-type enum's default/no-fallback guarantee.
/// </summary>
public sealed class WalletLocalizationTests : BunitContext
{
    public WalletLocalizationTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void BeeDayDashboardSkeleton_WalletSuppliesItsOwnAriaLabel_DoesNotFallBackToLoadingDashboard()
    {
        var cutEn = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<BeeDayDashboardSkeleton>(parameters => parameters
            .Add(component => component.AriaLabel, "Loading wallet")));
        Assert.Equal("Loading wallet", cutEn.Find("section.dashboard-skeleton").GetAttribute("aria-label"));

        var cutPt = Render<BeeDayDashboardSkeleton>(parameters => parameters
            .Add(component => component.AriaLabel, "Carregando carteira"));
        Assert.Equal("Carregando carteira", cutPt.Find("section.dashboard-skeleton").GetAttribute("aria-label"));
        Assert.DoesNotContain("Loading dashboard", cutPt.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void WalletSummary_UnderPortugueseUiCulture_RendersPortugueseLabels()
    {
        var summary = new WalletSummaryResponse(Guid.NewGuid(), 125.50m, 200m, 74.50m, 3, DateTimeOffset.UtcNow);

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<WalletSummary>(parameters => parameters
            .Add(component => component.Summary, summary)));

        Assert.Contains("Saldo atual", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Receita total", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Despesa total", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("3 transações", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void WalletFilters_UnderPortugueseUiCulture_RendersPortugueseLabelsAndOptions()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<WalletFilters>());

        Assert.Contains("Buscar", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Mais filtros", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("Descrição ou notas", cut.Find("input.beeday-field__control").GetAttribute("placeholder"));
    }

    [Fact]
    public void WalletEmptyState_UnderPortugueseUiCulture_RendersPortugueseCopy()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<WalletEmptyState>());

        Assert.Equal("Nenhuma transação encontrada", cut.Find(".beeday-empty-state__title").TextContent);
        Assert.Contains("Crie sua primeira transação", cut.Find(".beeday-empty-state__description").TextContent, StringComparison.Ordinal);
        Assert.Contains("Criar transação", cut.Find(".beeday-button--primary").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public void WalletTagManager_UnderPortugueseUiCulture_RendersPortugueseCopy()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<WalletTagManager>());

        Assert.Equal("Nenhuma tag ainda", cut.Find(".beeday-empty-state__title").TextContent);
        Assert.Contains("Nova tag", cut.Find(".wallet-panel-header .beeday-button--primary").TextContent, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US", "Income", "Expense")]
    [InlineData("pt-BR", "Receita", "Despesa")]
    public void TransactionCard_TransactionTypeFollowsCulture_ProvingNoRawEnumFallback(string culture, string incomeLabel, string expenseLabel)
    {
        // Under pt-BR, "Receita"/"Despesa" appearing (rather than the raw enum names "Income"/
        // "Expense") can only happen if the ternary picked the localized branch — a non-vacuous
        // proof that TransactionType never leaks its English enum name to a Portuguese UI.
        var income = CreateTransaction(TransactionType.Income);
        var expense = CreateTransaction(TransactionType.Expense);

        var (incomeMarkup, expenseMarkup) = BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            var incomeCut = Render<TransactionCard>(parameters => parameters.Add(component => component.Transaction, income));
            var expenseCut = Render<TransactionCard>(parameters => parameters.Add(component => component.Transaction, expense));
            return (incomeCut.Markup, expenseCut.Markup);
        });

        Assert.Contains(incomeLabel, incomeMarkup, StringComparison.Ordinal);
        Assert.Contains(expenseLabel, expenseMarkup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US", "Edit Transaction")]
    [InlineData("pt-BR", "Editar transação")]
    public void TransactionFormModal_TitleFollowsCulture(string culture, string expectedTitleFragment)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new TransactionFormModel())));

        Assert.Contains(expectedTitleFragment, cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US", "Create Tag")]
    [InlineData("pt-BR", "Criar tag")]
    public void TagFormModal_TitleFollowsCulture(string culture, string expectedTitleFragment)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new WalletTagFormModel())));

        Assert.Contains(expectedTitleFragment, cut.Markup, StringComparison.Ordinal);
    }

    private static TransactionResponse CreateTransaction(TransactionType type) =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Monthly salary", 1000m, 1000m, type,
            new DateOnly(2026, 7, 1), null, null, null, "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
