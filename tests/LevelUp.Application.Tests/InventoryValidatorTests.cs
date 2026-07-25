using FluentValidation;
using LevelUp.Application.Features.Inventory.Queries;
using LevelUp.Application.Features.Inventory.Requests;
using LevelUp.Application.Features.Inventory.Validation;
using LevelUp.Domain.Enums;
using Xunit;

namespace LevelUp.Application.Tests;

public sealed class InventoryValidatorTests
{
    [Fact]
    public async Task SaveTransaction_RejectsMoreThanTwoDecimalPlaces()
    {
        var request = new SaveTransactionRequest(
            "Item",
            10.999m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 25),
            null,
            null);
        var context = new ValidationContext<SaveTransactionRequest>(request);
        var result = await new SaveTransactionRequestValidator().ValidateAsync(
            context,
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SaveTag_RejectsInvalidColor()
    {
        var request = new SaveInventoryTagRequest("Food", "purple");
        var context = new ValidationContext<SaveInventoryTagRequest>(request);
        var result = await new SaveInventoryTagRequestValidator().ValidateAsync(
            context,
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task TransactionQuery_RejectsInvalidRanges()
    {
        var request = new GetTransactionsQuery(
            StartDate: new DateOnly(2026, 7, 25),
            EndDate: new DateOnly(2026, 7, 1),
            MinimumAmount: 100m,
            MaximumAmount: 10m);
        var context = new ValidationContext<GetTransactionsQuery>(request);
        var result = await new GetTransactionsQueryValidator().ValidateAsync(
            context,
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
