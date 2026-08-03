using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class TransactionTests
{
    [Theory]
    [InlineData(TransactionType.Income, 25.50, 25.50)]
    [InlineData(TransactionType.Expense, 25.50, -25.50)]
    public void Create_sets_signed_amount_from_type(TransactionType type, double amount, double expected)
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            "Transaction",
            (decimal)amount,
            type,
            new DateOnly(2026, 7, 24));

        Assert.Equal((decimal)expected, transaction.SignedAmount);
        Assert.Equal((decimal)amount, transaction.Amount);
    }

    [Fact]
    public void Create_normalizes_text_fields()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(),
            "  Internet   bill  ",
            80m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 24),
            notes: "  Monthly payment  ");

        Assert.Equal("Internet bill", transaction.Description);
        Assert.Equal("Monthly payment", transaction.Notes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10.999)]
    public void Create_rejects_invalid_amounts(double value)
    {
        Assert.Throws<DomainValidationException>(() => Transaction.Create(
            Guid.NewGuid(),
            "Invalid",
            (decimal)value,
            TransactionType.Expense,
            new DateOnly(2026, 7, 24)));
    }

    [Fact]
    public void Create_rejects_empty_description_invalid_type_and_date()
    {
        Assert.Throws<DomainValidationException>(() => Transaction.Create(
            Guid.NewGuid(), " ", 1m, TransactionType.Income, new DateOnly(2026, 7, 24)));
        Assert.Throws<DomainValidationException>(() => Transaction.Create(
            Guid.NewGuid(), "Invalid type", 1m, (TransactionType)999, new DateOnly(2026, 7, 24)));
        Assert.Throws<DomainValidationException>(() => Transaction.Create(
            Guid.NewGuid(), "Invalid date", 1m, TransactionType.Income, default));
    }

    [Fact]
    public void Update_preserves_identity_and_wallet()
    {
        var walletId = Guid.NewGuid();
        var transaction = Transaction.Create(
            walletId, "Old", 10m, TransactionType.Income, new DateOnly(2026, 7, 1));
        var id = transaction.Id;
        var createdAt = transaction.CreatedAtUtc;

        transaction.Update(
            "New", 15m, TransactionType.Expense, new DateOnly(2026, 7, 2), null, "Notes");

        Assert.Equal(id, transaction.Id);
        Assert.Equal(walletId, transaction.WalletId);
        Assert.Equal(createdAt, transaction.CreatedAtUtc);
        Assert.Equal("New", transaction.Description);
        Assert.Equal(-15m, transaction.SignedAmount);
        Assert.True(transaction.UpdatedAtUtc >= createdAt);
    }

    [Fact]
    public void Tag_can_be_assigned_and_removed()
    {
        var transaction = Transaction.Create(
            Guid.NewGuid(), "Tagged", 10m, TransactionType.Expense, new DateOnly(2026, 7, 1));
        var tagId = Guid.NewGuid();

        transaction.AssignTag(tagId);
        Assert.Equal(tagId, transaction.WalletTagId);

        transaction.RemoveTag();
        Assert.Null(transaction.WalletTagId);
    }
}
