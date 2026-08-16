using BeeDay.Application.Features.Wallets.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Services;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Wallet;

public sealed class TransactionCardTests : BunitContext
{
    public TransactionCardTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

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

        BunitLocalizationSupport.WithUiCulture("en-US", () =>
        {
            var cut = Render<TransactionCard>(parameters => parameters
                .Add(component => component.Transaction, transaction));

            var expectedAmount = WalletCurrencyFormatter.FormatSigned(transaction.Amount, isIncome: true);
            Assert.Equal(
                $"Edit Transaction: {transaction.Description}, {expectedAmount}",
                cut.Find("[role='button']").GetAttribute("aria-label"));
        });
    }

    [Theory]
    [InlineData("en-US", "7/1/2026")]
    [InlineData("pt-BR", "01/07/2026")]
    public void TransactionDate_UsesTheStandardShortDatePatternForTheCurrentCulture(string culture, string expectedDisplayDate)
    {
        // TransactionDate.ToString("d") — the standard short-date pattern — rather than a custom
        // "MMM dd, yyyy" pattern: a custom format string fixes day/month/year order and separators
        // regardless of culture (only token values like month names localize), so pt-BR would keep
        // rendering the en-US day-month order even with the right month name. "d" adapts the whole
        // structure, which is why en-US (month/day/year) and pt-BR (day/month/year) genuinely differ
        // here, not just in language.
        var transaction = CreateTransaction();

        BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            var cut = Render<TransactionCard>(parameters => parameters
                .Add(component => component.Transaction, transaction));

            var time = cut.Find("time");
            Assert.Equal(expectedDisplayDate, time.TextContent);
            Assert.Equal("2026-07-01", time.GetAttribute("datetime"));
        });
    }

    [Fact]
    public void PreservesDynamicTagColorAndDerivesReadableContrast()
    {
        var transaction = CreateTransaction() with
        {
            WalletTagId = Guid.NewGuid(),
            WalletTagName = "Custom",
            WalletTagColor = "#101010"
        };
        var cut = Render<TransactionCard>(parameters => parameters
            .Add(component => component.Transaction, transaction));

        var badgeStyle = cut.Find(".wallet-tag-badge").GetAttribute("style");
        Assert.Contains("background:#101010", badgeStyle, StringComparison.Ordinal);
        Assert.Contains("color:#ffffff", badgeStyle, StringComparison.Ordinal);
        Assert.Contains("border-color:#101010", badgeStyle, StringComparison.Ordinal);
    }

    private static TransactionResponse CreateTransaction() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "Monthly salary", 1000m, 1000m, TransactionType.Income,
            new DateOnly(2026, 7, 1), null, null, null, "", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
