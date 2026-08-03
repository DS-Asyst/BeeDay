using LevelUp.Web.Components.Features.Wallets.Components;
using LevelUp.Web.Components.Features.Wallets.Models;

namespace LevelUp.Web.Tests.Components.Wallet;

public sealed class TransactionFormModalTests : BunitContext
{
    [Fact]
    public void ShowsDeleteOnlyWhenEditing()
    {
        var editingCut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new TransactionFormModel()));

        Assert.NotEmpty(editingCut.FindAll(".editor-modal__footer-danger .levelup-button--danger"));

        var creatingCut = Render<TransactionFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new TransactionFormModel()));

        Assert.Empty(creatingCut.FindAll(".editor-modal__footer-danger .levelup-button--danger"));
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

        await cut.Find(".editor-modal__footer-danger .levelup-button--danger").ClickAsync();

        Assert.True(deleteRequested);
        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }
}
