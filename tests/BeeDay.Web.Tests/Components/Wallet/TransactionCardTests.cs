using LevelUp.Application.Features.Wallets.Responses;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Wallets.Components;
using LevelUp.Web.Components.Features.Wallets.Services;

namespace LevelUp.Web.Tests.Components.Wallet;

public sealed class TransactionCardTests : BunitContext
{
    [Fact]
    public void ClickOnCardBody_InvokesOnEdit()
    {
        var transaction = CreateTransaction();
        TransactionResponse? edited = null;
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, transaction)
            .Add(component => component.OnEdit, (TransactionResponse value) => edited = value));

        cut.Find("[role='button']").Click();

        Assert.Equal(transaction, edited);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void ActivationKeyOnBody_InvokesOnEdit(string key)
    {
        var editInvoked = false;
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, CreateTransaction())
            .Add(component => component.OnEdit, (TransactionResponse _) => editInvoked = true));

        cut.Find("[role='button']").KeyDown(key);

        Assert.True(editInvoked);
    }

    [Fact]
    public void OtherKeyOnBody_DoesNotInvokeOnEdit()
    {
        var editInvoked = false;
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, CreateTransaction())
            .Add(component => component.OnEdit, (TransactionResponse _) => editInvoked = true));

        cut.Find("[role='button']").KeyDown("Tab");

        Assert.False(editInvoked);
    }

    [Fact]
    public void DoesNotRenderTheLegacyThreeDotMenu()
    {
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, CreateTransaction()));

        Assert.DoesNotContain("activity-card__menu", cut.Markup, StringComparison.Ordinal);
        Assert.Empty(cut.FindAll(".card-action-menu__panel"));
    }

    [Fact]
    public void ExposesAccessibleEditName()
    {
        var transaction = CreateTransaction();
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, transaction));

        var expectedAmount = WalletCurrencyFormatter.FormatSigned(transaction.Amount, isIncome: true);
        Assert.Equal(
            $"Edit Transaction: {transaction.Description}, {expectedAmount}",
            cut.Find("[role='button']").GetAttribute("aria-label"));
    }

    private static TransactionResponse CreateTransaction() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Monthly salary", 1000m, 1000m, TransactionType.Income,
            new DateOnly(2026, 7, 1), null, null, null, "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
