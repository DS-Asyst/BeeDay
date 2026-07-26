using LevelUp.Application.Features.Inventory.Responses;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Inventory.Components;

namespace LevelUp.Web.Tests.Components.Inventory;

public sealed class InventoryFiltersTests : BunitContext
{
    [Fact]
    public void RendersAllFilterControlsAndActiveIndicator()
    {
        var tag = CreateTag("Food");
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.Search, "rent")
            .Add(component => component.TypeFilter, "Expense")
            .Add(component => component.TagFilter, tag.Id.ToString())
            .Add(component => component.StartDate, new DateOnly(2026, 1, 1))
            .Add(component => component.EndDate, new DateOnly(2026, 1, 31))
            .Add(component => component.ActiveFilterCount, 5)
            .Add(component => component.Tags, [tag]));

        Assert.Equal(6, cut.FindAll("input, select").Count);
        Assert.Contains("5 active filters", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Food", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void InvokesFilterCallbacks()
    {
        string? search = null;
        string? type = null;
        DateOnly? startDate = null;
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.SearchChanged, value => search = value)
            .Add(component => component.TypeFilterChanged, value => type = value)
            .Add(component => component.StartDateChanged, value => startDate = value));

        cut.Find("input[placeholder='Description or notes']").Input("salary");
        cut.FindAll("select")[0].Change("Income");
        cut.Find("input[type='date']").Change("2026-07-01");

        Assert.Equal("salary", search);
        Assert.Equal("Income", type);
        Assert.Equal(new DateOnly(2026, 7, 1), startDate);
    }

    [Fact]
    public void DisablesEveryControlDuringOperation()
    {
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.IsDisabled, true)
            .Add(component => component.ActiveFilterCount, 1));

        Assert.All(cut.FindAll("input, select, button"), element => Assert.True(element.HasAttribute("disabled")));
    }

    private static InventoryTagResponse CreateTag(string name) =>
        new(Guid.NewGuid(), name, "#7A4FCB", 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}

public sealed class InventoryEmptyStateTests : BunitContext
{
    [Fact]
    public void RendersCreateActionWhenInventoryIsEmpty()
    {
        var invoked = false;
        var cut = Render<InventoryEmptyState>(parameters => parameters
            .Add(component => component.OnCreateTransaction, () => invoked = true));

        Assert.Contains("Create your first transaction", cut.Markup, StringComparison.Ordinal);
        cut.Find("button").Click();
        Assert.True(invoked);
    }

    [Fact]
    public void RendersClearActionWhenFiltersHaveNoResults()
    {
        var invoked = false;
        var cut = Render<InventoryEmptyState>(parameters => parameters
            .Add(component => component.HasFilters, true)
            .Add(component => component.OnClearFilters, () => invoked = true));

        Assert.Contains("Change or clear your filters", cut.Markup, StringComparison.Ordinal);
        cut.Find("button").Click();
        Assert.True(invoked);
    }
}

public sealed class TransactionListTests : BunitContext
{
    [Fact]
    public void RendersRefreshStateAndTransactions()
    {
        var transaction = CreateTransaction();
        var response = new PagedTransactionsResponse([transaction], 1, 10, 1, 1);
        var cut = Render<TransactionList>(parameters => parameters
            .Add(component => component.Transactions, response)
            .Add(component => component.IsRefreshing, true));

        Assert.True(cut.Find(".inventory-transaction-list").HasAttribute("aria-busy"));
        Assert.Contains(transaction.Description, cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DisablesPaginationWhileBusy()
    {
        var response = new PagedTransactionsResponse([CreateTransaction()], 1, 1, 2, 2);
        var cut = Render<TransactionList>(parameters => parameters
            .Add(component => component.Transactions, response)
            .Add(component => component.IsBusy, true));

        Assert.All(cut.FindAll(".inventory-pagination button"), button => Assert.True(button.HasAttribute("disabled")));
    }

    private static TransactionResponse CreateTransaction() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Monthly salary", 1000m, 1000m, TransactionType.Income,
            new DateOnly(2026, 7, 1), null, null, null, "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
