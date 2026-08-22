using System.ComponentModel.DataAnnotations;
using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Wallet;

public sealed class WalletComponentTests : BunitContext
{
    public WalletComponentTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

    [Fact]
    public void Summary_RendersWalletTotals()
    {
        var summary = new WalletSummaryResponse(Guid.NewGuid(), 125.50m, 200m, 74.50m, 3, DateTimeOffset.UtcNow);
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<WalletSummary>(parameters => parameters.Add(component => component.Summary, summary)));

        Assert.Contains("$125.50", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("$200.00", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("$74.50", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("3 transactions", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Summary_CardsComposeTheSharedStaticBeeDayCardPrimitive()
    {
        var summary = new WalletSummaryResponse(Guid.NewGuid(), 125.50m, 200m, 74.50m, 3, DateTimeOffset.UtcNow);
        var cut = Render<WalletSummary>(parameters => parameters.Add(component => component.Summary, summary));

        var cards = cut.FindAll(".wallet-summary__card");
        Assert.Equal(3, cards.Count);
        Assert.All(cards, card => Assert.Contains("beeday-card", card.ClassList));
    }

    [Fact]
    public void TransactionForm_RejectsZeroAmount()
    {
        var model = new TransactionFormModel { Description = "Test transaction", Amount = 0m };
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.False(valid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(TransactionFormModel.Amount)));
    }

    [Theory]
    [InlineData("#5247F9", true)] // Cor0 — one of the 10 official palette colors (EPIC 27 Sprint 27.11).
    [InlineData("#7A4FCB", false)] // A real hex color, but not one of the 10 — no longer accepted.
    [InlineData("purple", false)]
    public void TagForm_ValidatesHexColor(string color, bool expected)
    {
        var model = new WalletTagFormModel { Name = "Food", Color = color };
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.Equal(expected, valid);
    }

    [Fact]
    public void CurrencyFormatter_UnderEnglishUiCulture_UsesEnUsGroupingWithUsdSymbol()
    {
        BunitLocalizationSupport.WithUiCulture("en-US", () =>
        {
            Assert.Equal("$125.50", BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(125.50m));
            Assert.Equal("-$89.90", BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(-89.90m));
            Assert.Equal("$1,234.56", BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(1234.56m));
        });
    }

    [Fact]
    public void CurrencyFormatter_UnderPortugueseUiCulture_UsesPortugueseGroupingButStillUsdSymbol()
    {
        BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
        {
            var formatted = BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(1234.56m);

            // Presentation (grouping/decimal separators) follows pt-BR; the currency itself stays
            // USD ("$") regardless of UI culture — pt-BR's own "C" format would otherwise render
            // "R$", which would incorrectly imply the underlying financial data changed currency.
            Assert.Contains('$', formatted);
            Assert.DoesNotContain("R$", formatted, StringComparison.Ordinal);
            Assert.Contains("1.234,56", formatted, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void CurrencyFormatter_PreservesTheFinancialValueAcrossCultures()
    {
        // Same underlying decimal, different presentation — proves culture only changes how the
        // number is *displayed*, never the value itself or the currency it represents.
        const decimal value = 1234.56m;

        var enUs = BunitLocalizationSupport.WithUiCulture("en-US", () => BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(value));
        var ptBr = BunitLocalizationSupport.WithUiCulture("pt-BR", () => BeeDay.Web.Components.Features.Wallets.Services.WalletCurrencyFormatter.Format(value));

        Assert.NotEqual(enUs, ptBr);
        Assert.Equal(
            decimal.Parse(enUs.Replace("$", string.Empty).Trim(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("en-US")),
            decimal.Parse(ptBr.Replace("$", string.Empty).Trim(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.GetCultureInfo("pt-BR")));
    }

    [Theory]
    [InlineData("#FFFFFF", "#17111f")]
    [InlineData("#111111", "#ffffff")]
    [InlineData("invalid", "#ffffff")]
    public void TagContrastCalculator_ReturnsReadableText(string color, string expected)
    {
        Assert.Equal(expected, BeeDay.Web.Components.Features.Wallets.Services.TagContrastCalculator.GetTextColor(color));
    }
}

public sealed class WalletPageStateTests
{
    [Fact]
    public void ClearFilters_ResetsFilterValuesAndPage()
    {
        var state = new BeeDay.Web.Components.Features.Wallets.State.WalletPageState
        {
            Search = "rent",
            TypeFilter = "Expense",
            TagFilter = Guid.NewGuid().ToString(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31),
            Sort = "amount-desc",
            Page = 3
        };

        state.ClearFilters();

        Assert.False(state.HasFilters);
        Assert.Equal(0, state.ActiveFilterCount);
        Assert.Null(state.StartDate);
        Assert.Null(state.EndDate);
        Assert.Equal(1, state.Page);
        Assert.Equal("amount-desc", state.Sort);
    }

    [Fact]
    public void ActiveFilterCount_CountsSearchTypeTagAndDateBounds()
    {
        var state = new BeeDay.Web.Components.Features.Wallets.State.WalletPageState
        {
            Search = "rent",
            TypeFilter = "Expense",
            TagFilter = Guid.NewGuid().ToString(),
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 1, 31)
        };

        Assert.True(state.HasFilters);
        Assert.Equal(5, state.ActiveFilterCount);
    }
}

public sealed class WalletInteractionStateTests
{
    [Fact]
    public void TryBegin_PreventsConcurrentOperations()
    {
        var state = new BeeDay.Web.Components.Features.Wallets.State.WalletInteractionState();

        Assert.True(state.TryBegin("save-transaction"));
        Assert.True(state.IsBusy);
        Assert.Equal("save-transaction", state.Operation);
        Assert.False(state.TryBegin("delete-transaction"));

        state.End();

        Assert.False(state.IsBusy);
        Assert.Null(state.Operation);
        Assert.True(state.TryBegin("delete-transaction"));
    }
}
