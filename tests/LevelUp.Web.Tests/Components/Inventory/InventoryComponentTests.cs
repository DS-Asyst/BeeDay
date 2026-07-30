using System.ComponentModel.DataAnnotations;
using LevelUp.Application.Features.Inventory.Responses;
using LevelUp.Web.Components.Features.Inventory.Components;
using LevelUp.Web.Components.Features.Inventory.Models;

namespace LevelUp.Web.Tests.Components.Inventory;

public sealed class InventoryComponentTests : BunitContext
{
    [Fact]
    public void Summary_RendersWalletTotals()
    {
        var summary = new WalletSummaryResponse(Guid.NewGuid(), 125.50m, 200m, 74.50m, 3, DateTimeOffset.UtcNow);
        var cut = Render<WalletSummary>(parameters => parameters.Add(component => component.Summary, summary));

        Assert.Contains("$125.50", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("$200.00", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("$74.50", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("3 transactions", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void Summary_CardsComposeTheSharedStaticLevelUpCardPrimitive()
    {
        var summary = new WalletSummaryResponse(Guid.NewGuid(), 125.50m, 200m, 74.50m, 3, DateTimeOffset.UtcNow);
        var cut = Render<WalletSummary>(parameters => parameters.Add(component => component.Summary, summary));

        var cards = cut.FindAll(".inventory-summary__card");
        Assert.Equal(3, cards.Count);
        Assert.All(cards, card => Assert.Contains("levelup-card", card.ClassList));
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
    [InlineData("#7A4FCB", true)]
    [InlineData("purple", false)]
    public void TagForm_ValidatesHexColor(string color, bool expected)
    {
        var model = new InventoryTagFormModel { Name = "Food", Color = color };
        var results = new List<ValidationResult>();
        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.Equal(expected, valid);
    }

    [Fact]
    public void CurrencyFormatter_UsesEnUs()
    {
        Assert.Equal("$125.50", LevelUp.Web.Components.Features.Inventory.Services.InventoryCurrencyFormatter.Format(125.50m));
        Assert.Equal("-$89.90", LevelUp.Web.Components.Features.Inventory.Services.InventoryCurrencyFormatter.Format(-89.90m));
    }

    [Theory]
    [InlineData("#FFFFFF", "#17111f")]
    [InlineData("#111111", "#ffffff")]
    [InlineData("invalid", "#ffffff")]
    public void TagContrastCalculator_ReturnsReadableText(string color, string expected)
    {
        Assert.Equal(expected, LevelUp.Web.Components.Features.Inventory.Services.TagContrastCalculator.GetTextColor(color));
    }
}

public sealed class InventoryPageStateTests
{
    [Fact]
    public void ClearFilters_ResetsFilterValuesAndPage()
    {
        var state = new LevelUp.Web.Components.Features.Inventory.State.InventoryPageState
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
        var state = new LevelUp.Web.Components.Features.Inventory.State.InventoryPageState
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

public sealed class InventoryInteractionStateTests
{
    [Fact]
    public void TryBegin_PreventsConcurrentOperations()
    {
        var state = new LevelUp.Web.Components.Features.Inventory.State.InventoryInteractionState();

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
