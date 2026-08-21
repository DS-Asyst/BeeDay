using BeeDay.Application.Features.Wallets.Queries;
using BeeDay.Application.Features.Wallets.Requests;
using BeeDay.Application.Features.Wallets.Validation;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using FluentValidation;

namespace BeeDay.Application.Tests;

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

    // EPIC 30 Sprint 30.15 / BD30-F058: this validator previously had no upper bound at all — only
    // Transaction.ValidateAmount (Domain) rejected an over-limit Amount, so a direct-EF/bulk-import
    // write bypassing Domain would not have been caught here either. Domain's own boundary is proven
    // separately in TransactionTests; this proves this specific FluentValidation rule.
    [Fact]
    public async Task SaveTransaction_RejectsAmountAboveTheMaximum()
    {
        var request = new SaveTransactionRequest(
            "Item",
            Transaction.MaximumAmount + 0.01m,
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
    public async Task SaveTransaction_AcceptsAmountExactlyAtTheMaximum()
    {
        var request = new SaveTransactionRequest(
            "Item",
            Transaction.MaximumAmount,
            TransactionType.Expense,
            new DateOnly(2026, 7, 25),
            null,
            null);
        var context = new ValidationContext<SaveTransactionRequest>(request);
        var result = await new SaveTransactionRequestValidator().ValidateAsync(
            context,
            TestContext.Current.CancellationToken);
        Assert.True(result.IsValid);
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
