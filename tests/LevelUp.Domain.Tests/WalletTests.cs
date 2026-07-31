using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class WalletTests
{
    [Fact]
    public void Create_requires_user_identifier()
    {
        Assert.Throws<DomainValidationException>(() => Wallet.Create(Guid.Empty));
    }

    [Fact]
    public void Empty_wallet_has_zero_totals()
    {
        var wallet = Wallet.Create(Guid.NewGuid());

        Assert.Equal(0m, wallet.CalculateBalance([]));
        Assert.Equal(0m, wallet.CalculateTotalIncome([]));
        Assert.Equal(0m, wallet.CalculateTotalExpenses([]));
    }

    [Fact]
    public void Balance_is_calculated_from_income_and_expenses()
    {
        var wallet = Wallet.Create(Guid.NewGuid());
        var transactions = new[]
        {
            Transaction.Create(wallet.Id, "Salary", 1500m, TransactionType.Income, new DateOnly(2026, 7, 1)),
            Transaction.Create(wallet.Id, "Internet", 80m, TransactionType.Expense, new DateOnly(2026, 7, 10)),
            Transaction.Create(wallet.Id, "Food", 120m, TransactionType.Expense, new DateOnly(2026, 7, 11))
        };

        Assert.Equal(1300m, wallet.CalculateBalance(transactions));
        Assert.Equal(1500m, wallet.CalculateTotalIncome(transactions));
        Assert.Equal(200m, wallet.CalculateTotalExpenses(transactions));
    }

    [Fact]
    public void Balance_can_be_negative()
    {
        var wallet = Wallet.Create(Guid.NewGuid());
        var expense = Transaction.Create(
            wallet.Id,
            "Rent",
            900m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 1));

        Assert.Equal(-900m, wallet.CalculateBalance([expense]));
    }

    [Fact]
    public void Calculations_ignore_transactions_from_another_wallet()
    {
        var wallet = Wallet.Create(Guid.NewGuid());
        var anotherWallet = Wallet.Create(Guid.NewGuid());
        var ownIncome = Transaction.Create(wallet.Id, "Salary", 100m, TransactionType.Income, new DateOnly(2026, 7, 1));
        var foreignIncome = Transaction.Create(anotherWallet.Id, "Other", 500m, TransactionType.Income, new DateOnly(2026, 7, 1));

        Assert.Equal(100m, wallet.CalculateBalance([ownIncome, foreignIncome]));
    }
}
