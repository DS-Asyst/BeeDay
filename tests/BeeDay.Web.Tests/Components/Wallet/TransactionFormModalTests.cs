using System.ComponentModel.DataAnnotations;
using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Wallet;

public sealed class TransactionFormModalTests : BunitContext
{
    public TransactionFormModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ShowsDeleteOnlyWhenEditing()
    {
        var editingCut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new TransactionFormModel()));

        Assert.NotEmpty(editingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));

        var creatingCut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new TransactionFormModel()));

        Assert.Empty(creatingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));
    }

    [Fact]
    public async Task ClickingDelete_InvokesOnDeleteRequested_WithoutASecondConfirmationDialog()
    {
        var deleteRequested = false;
        var cut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new TransactionFormModel())
            .Add(component => component.OnDeleteRequested, () => deleteRequested = true));

        await cut.Find(".editor-modal__footer-danger .beeday-button--danger").ClickAsync();

        Assert.True(deleteRequested);
        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }

    [Fact]
    public void UsesTheCentralWalletIconInsteadOfALocalFunctionalSymbol()
    {
        var cut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Model, new TransactionFormModel()));

        var icon = cut.Find(".editor-modal__type-summary .beeday-icon");
        Assert.Equal("Wallet", icon.GetAttribute("data-icon"));
        Assert.DoesNotContain("◇", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("pt-BR")]
    public void AmountRange_ValidatesMinimumWithoutDependingOnCurrentCulture(string culture)
    {
        BunitLocalizationSupport.WithUiCulture(culture, () =>
        {
            var model = CreateValidModel(0.01m);
            var validationContext = new ValidationContext(model)
            {
                MemberName = nameof(TransactionFormModel.Amount)
            };

            Assert.True(Validator.TryValidateProperty(
                model.Amount,
                validationContext,
                validationResults: null));

            model.Amount = 0m;

            Assert.False(Validator.TryValidateProperty(
                model.Amount,
                validationContext,
                validationResults: null));
        });
    }

    private static TransactionFormModel CreateValidModel(decimal amount) => new()
    {
        Description = "Coffee",
        Amount = amount,
        TransactionDate = new DateTime(2026, 8, 20)
    };
}
