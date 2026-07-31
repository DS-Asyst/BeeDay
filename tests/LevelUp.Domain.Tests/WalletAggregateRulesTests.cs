using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class WalletAggregateRulesTests
{
    [Fact]
    public void User_can_have_only_one_wallet()
    {
        var data = CreateDataWithCurrentUser(out var user);
        data.AddWallet(Wallet.Create(user.Id));

        Assert.Throws<InvalidDomainStateException>(() => data.AddWallet(Wallet.Create(user.Id)));
    }

    [Fact]
    public void Tag_names_are_unique_per_user_ignoring_case()
    {
        var data = CreateDataWithCurrentUser(out var user);
        data.AddWalletTag(WalletTag.Create(user.Id, "Food"));

        Assert.Throws<InvalidDomainStateException>(() =>
            data.AddWalletTag(WalletTag.Create(user.Id, "food")));
    }

    [Fact]
    public void Transaction_tag_must_belong_to_wallet_owner()
    {
        var data = new LevelUpData();
        var firstUser = User.Create("First", "first@levelup.invalid");
        var secondUser = User.Create("Second", "second@levelup.invalid");
        data.AddUser(firstUser);
        data.AddUser(secondUser);
        var wallet = Wallet.Create(firstUser.Id);
        data.AddWallet(wallet);
        var foreignTag = WalletTag.Create(secondUser.Id, "Foreign");
        data.AddWalletTag(foreignTag);
        var transaction = Transaction.Create(
            wallet.Id, "Invalid", 10m, TransactionType.Expense, new DateOnly(2026, 7, 1), foreignTag.Id);

        Assert.Throws<InvalidDomainStateException>(() => data.AddTransaction(transaction));
    }

    [Fact]
    public void Removing_tag_preserves_transactions_and_clears_association()
    {
        var data = CreateDataWithCurrentUser(out var user);
        var wallet = Wallet.Create(user.Id);
        var tag = WalletTag.Create(user.Id, "Food");
        data.AddWallet(wallet);
        data.AddWalletTag(tag);
        var transaction = Transaction.Create(
            wallet.Id, "Lunch", 25m, TransactionType.Expense, new DateOnly(2026, 7, 1), tag.Id);
        data.AddTransaction(transaction);

        data.RemoveWalletTag(tag.Id);

        Assert.Empty(data.WalletTags);
        Assert.Single(data.Transactions);
        Assert.Null(transaction.WalletTagId);
    }

    private static LevelUpData CreateDataWithCurrentUser(out User user)
    {
        var data = new LevelUpData();
        user = User.Create("User", "user@levelup.invalid");
        data.AddUser(user);
        return data;
    }
}
