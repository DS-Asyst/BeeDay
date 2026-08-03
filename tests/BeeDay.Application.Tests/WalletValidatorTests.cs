using FluentValidation;
using LevelUp.Application.Features.Wallets.Queries;
using LevelUp.Application.Features.Wallets.Requests;
using LevelUp.Application.Features.Wallets.Validation;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Tests;

public sealed class WalletValidatorTests
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
        var request = new SaveWalletTagRequest("Food", "purple");
        var context = new ValidationContext<SaveWalletTagRequest>(request);
        var result = await new SaveWalletTagRequestValidator().ValidateAsync(
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
