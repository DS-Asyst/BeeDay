using LevelUp.Application.Features.Inventory.Responses;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Inventory.Components;

namespace LevelUp.Web.Tests.Components.Inventory;

public sealed class InventoryFiltersTests : BunitContext
{
    [Fact]
    public void RendersAllFilterControlsAndActiveIndicatorWhenSecondaryFiltersAreActive()
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
        Assert.Equal("true", cut.Find(".inventory-filter-toggle").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task InvokesFilterCallbacks()
    {
        string? search = null;
        string? type = null;
        DateOnly? startDate = null;
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.SearchChanged, value => search = value)
            .Add(component => component.TypeFilterChanged, value => type = value)
            .Add(component => component.StartDateChanged, value => startDate = value));

        cut.Find("input[placeholder='Description or notes']").Input("salary");

        await cut.Find(".inventory-filter-toggle").ClickAsync();

        cut.Find(".inventory-secondary-filters select").Change("Income");
        cut.Find(".inventory-secondary-filters input[type='date']").Change("2026-07-01");

        Assert.Equal("salary", search);
        Assert.Equal("Income", type);
        Assert.Equal(new DateOnly(2026, 7, 1), startDate);
    }

    [Fact]
    public void DisablesEveryVisibleControlDuringOperation()
    {
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.IsDisabled, true)
            .Add(component => component.TypeFilter, "Expense")
            .Add(component => component.ActiveFilterCount, 1));

        Assert.All(cut.FindAll("input, select, button"), element => Assert.True(element.HasAttribute("disabled")));
    }

    [Fact]
    public void ToggleStartsCollapsedWhenNoSecondaryFilterIsActive()
    {
        var cut = Render<InventoryFilters>();

        var toggle = cut.Find(".inventory-filter-toggle");
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));

        var secondary = cut.Find("#inventory-secondary-filters");
        Assert.True(secondary.HasAttribute("hidden"));
        Assert.Empty(secondary.QuerySelectorAll("input, select, button"));
    }

    [Theory]
    [InlineData(nameof(InventoryFilters.TypeFilter))]
    [InlineData(nameof(InventoryFilters.StartDate))]
    [InlineData(nameof(InventoryFilters.EndDate))]
    [InlineData(nameof(InventoryFilters.Sort))]
    public void ToggleStartsExpandedWhenASecondaryFilterIsAlreadyActive(string activeParameter)
    {
        var cut = Render<InventoryFilters>(parameters =>
        {
            switch (activeParameter)
            {
                case nameof(InventoryFilters.TypeFilter):
                    parameters.Add(component => component.TypeFilter, "Expense");
                    break;
                case nameof(InventoryFilters.StartDate):
                    parameters.Add(component => component.StartDate, new DateOnly(2026, 1, 1));
                    break;
                case nameof(InventoryFilters.EndDate):
                    parameters.Add(component => component.EndDate, new DateOnly(2026, 1, 31));
                    break;
                case nameof(InventoryFilters.Sort):
                    parameters.Add(component => component.Sort, "amount-desc");
                    break;
            }
        });

        Assert.Equal("true", cut.Find(".inventory-filter-toggle").GetAttribute("aria-expanded"));
        Assert.False(cut.Find("#inventory-secondary-filters").HasAttribute("hidden"));
    }

    [Fact]
    public async Task ClickingTheToggleExpandsAndCollapsesTheSecondarySection()
    {
        var cut = Render<InventoryFilters>();
        var toggle = cut.Find(".inventory-filter-toggle");

        await toggle.ClickAsync();
        Assert.Equal("true", cut.Find(".inventory-filter-toggle").GetAttribute("aria-expanded"));
        Assert.False(cut.Find("#inventory-secondary-filters").HasAttribute("hidden"));
        Assert.NotEmpty(cut.Find("#inventory-secondary-filters").QuerySelectorAll("select"));

        await cut.Find(".inventory-filter-toggle").ClickAsync();
        Assert.Equal("false", cut.Find(".inventory-filter-toggle").GetAttribute("aria-expanded"));
        Assert.True(cut.Find("#inventory-secondary-filters").HasAttribute("hidden"));
        Assert.Empty(cut.Find("#inventory-secondary-filters").QuerySelectorAll("select, input, button"));
    }

    [Fact]
    public void ToggleAriaControlsReferencesTheSecondaryFilterContainerId()
    {
        var cut = Render<InventoryFilters>();

        var toggle = cut.Find(".inventory-filter-toggle");
        var secondary = cut.Find("#inventory-secondary-filters");

        Assert.Equal(secondary.GetAttribute("id"), toggle.GetAttribute("aria-controls"));
    }

    [Fact]
    public void DoesNotRenderANumericBadgeEvenWhenSecondaryFiltersAreActive()
    {
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.TypeFilter, "Expense")
            .Add(component => component.StartDate, new DateOnly(2026, 1, 1))
            .Add(component => component.EndDate, new DateOnly(2026, 1, 31))
            .Add(component => component.Sort, "amount-desc"));

        Assert.Empty(cut.FindAll(".inventory-filter-toggle__count"));
        Assert.Contains("More Filters", cut.Find(".inventory-filter-toggle").TextContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ClearFiltersInvokesTheCallbackWhenClicked()
    {
        var invoked = false;
        var cut = Render<InventoryFilters>(parameters => parameters
            .Add(component => component.TypeFilter, "Expense")
            .Add(component => component.ActiveFilterCount, 1)
            .Add(component => component.OnClearFilters, () => invoked = true));

        await cut.Find(".inventory-active-filters button").ClickAsync();

        Assert.True(invoked);
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

        Assert.Equal("No transactions found", cut.Find(".levelup-empty-state__title").TextContent);
        Assert.Contains("Create your first transaction", cut.Find(".levelup-empty-state__description").TextContent, StringComparison.Ordinal);

        var button = cut.Find(".levelup-button--primary");
        Assert.Contains("Create transaction", button.TextContent, StringComparison.Ordinal);
        button.Click();
        Assert.True(invoked);
    }

    [Fact]
    public void RendersClearActionWhenFiltersHaveNoResults()
    {
        var invoked = false;
        var cut = Render<InventoryEmptyState>(parameters => parameters
            .Add(component => component.HasFilters, true)
            .Add(component => component.OnClearFilters, () => invoked = true));

        Assert.Contains("Change or clear your filters", cut.Find(".levelup-empty-state__description").TextContent, StringComparison.Ordinal);
        cut.Find("button").Click();
        Assert.True(invoked);
    }
}

public sealed class TransactionListTests : BunitContext
{
    public TransactionListTests() =>
        Services.AddScoped<LevelUp.Web.Services.CardActionMenuCoordinator>();

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

    [Fact]
    public void MarksThePanelEmptyOnlyWhenThereAreNoTransactions()
    {
        var emptyResponse = new PagedTransactionsResponse([], 1, 20, 0, 0);
        var emptyCut = Render<TransactionList>(parameters => parameters
            .Add(component => component.Transactions, emptyResponse));

        Assert.Contains("inventory-main-panel--empty", emptyCut.Find("section.inventory-main-panel").ClassList);

        var populatedResponse = new PagedTransactionsResponse([CreateTransaction()], 1, 20, 1, 1);
        var populatedCut = Render<TransactionList>(parameters => parameters
            .Add(component => component.Transactions, populatedResponse));

        Assert.DoesNotContain("inventory-main-panel--empty", populatedCut.Find("section.inventory-main-panel").ClassList);
    }

    private static TransactionResponse CreateTransaction() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Monthly salary", 1000m, 1000m, TransactionType.Income,
            new DateOnly(2026, 7, 1), null, null, null, "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}

public sealed class InventoryTagManagerTests : BunitContext
{
    [Fact]
    public void RendersTheSharedEmptyStateWhenNoTagsExist()
    {
        var cut = Render<InventoryTagManager>();

        Assert.Equal("No tags yet", cut.Find(".levelup-empty-state__title").TextContent);
        Assert.Contains(
            "Create one to organize your income and expenses.",
            cut.Find(".levelup-empty-state__description").TextContent,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ClickingNewTagInvokesOnCreate()
    {
        var invoked = false;
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.OnCreate, () => invoked = true));

        await cut.Find(".inventory-panel-header .levelup-button--primary").ClickAsync();

        Assert.True(invoked);
    }

    [Fact]
    public async Task ClickingTheTagRowInvokesOnEditWithThatTag()
    {
        var tag = CreateTag("Groceries");
        InventoryTagResponse? edited = null;
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag])
            .Add(component => component.OnEdit, (InventoryTagResponse value) => edited = value));

        await cut.Find("[role='button']").ClickAsync();

        Assert.Equal(tag, edited);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void ActivationKeyOnTheTagRowInvokesOnEdit(string key)
    {
        var tag = CreateTag("Groceries");
        var editInvoked = false;
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag])
            .Add(component => component.OnEdit, (InventoryTagResponse _) => editInvoked = true));

        cut.Find("[role='button']").KeyDown(key);

        Assert.True(editInvoked);
    }

    [Fact]
    public void OtherKeyOnTheTagRowDoesNotInvokeOnEdit()
    {
        var tag = CreateTag("Groceries");
        var editInvoked = false;
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag])
            .Add(component => component.OnEdit, (InventoryTagResponse _) => editInvoked = true));

        cut.Find("[role='button']").KeyDown("Tab");

        Assert.False(editInvoked);
    }

    [Fact]
    public void TagRowRendersNoEditOrDeleteButtons()
    {
        var tag = CreateTag("Groceries");
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag]));

        Assert.Empty(cut.Find(".inventory-tag-item").QuerySelectorAll("button"));
    }

    [Fact]
    public void RendersNoModal()
    {
        // The editor modal is owned and rendered by the page (Inventory.razor), not by this
        // list component — nesting it here previously trapped its fixed-position backdrop
        // inside the page's animated <main>, clipping it to a rectangle instead of the viewport.
        var tag = CreateTag("Groceries");
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag]));

        Assert.Empty(cut.FindAll(".editor-modal"));
    }

    [Fact]
    public void ExposesAccessibleEditNameOnTheTagRow()
    {
        var tag = CreateTag("Groceries");
        var cut = Render<InventoryTagManager>(parameters => parameters
            .Add(component => component.Tags, [tag]));

        Assert.Equal("Edit Tag: Groceries", cut.Find("[role='button']").GetAttribute("aria-label"));
    }

    private static InventoryTagResponse CreateTag(string name) =>
        new(Guid.NewGuid(), name, "#7A4FCB", 0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
