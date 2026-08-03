using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Models;

namespace BeeDay.Web.Tests.Components.Wallet;

public sealed class TagFormModalTests : BunitContext
{
    [Fact]
    public void ShowsDeleteOnlyWhenEditing()
    {
        var editingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.NotEmpty(editingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));

        var creatingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.Empty(creatingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));
    }

    [Fact]
    public async Task ClickingDelete_InvokesOnDeleteRequested_WithoutASecondConfirmationDialog()
    {
        var deleteRequested = false;
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new WalletTagFormModel())
            .Add(component => component.OnDeleteRequested, () => deleteRequested = true));

        await cut.Find(".editor-modal__footer-danger .beeday-button--danger").ClickAsync();

        Assert.True(deleteRequested);
        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }

    [Fact]
    public void RendersAsASiblingShapeMatchingTheSharedEditorModalShell()
    {
        // Uses the exact same EditorModalShell markup contract as every other editor modal
        // (Transaction, Project, Habit, Task, To-Do) — .editor-modal-backdrop / .editor-modal —
        // with no Wallet-specific modal classes.
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.NotEmpty(cut.FindAll(".editor-modal-backdrop"));
        Assert.NotEmpty(cut.FindAll(".editor-modal"));
        Assert.Empty(cut.FindAll(".wallet-modal"));
        Assert.Empty(cut.FindAll(".wallet-modal-backdrop"));
    }
}
