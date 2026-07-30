using LevelUp.Web.Components.Features.Inventory.Components;
using LevelUp.Web.Components.Features.Inventory.Models;

namespace LevelUp.Web.Tests.Components.Inventory;

public sealed class TagFormModalTests : BunitContext
{
    [Fact]
    public void ShowsDeleteOnlyWhenEditing()
    {
        var editingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new InventoryTagFormModel()));

        Assert.NotEmpty(editingCut.FindAll(".editor-modal__footer-danger .levelup-button--danger"));

        var creatingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new InventoryTagFormModel()));

        Assert.Empty(creatingCut.FindAll(".editor-modal__footer-danger .levelup-button--danger"));
    }

    [Fact]
    public async Task ClickingDelete_InvokesOnDeleteRequested_WithoutASecondConfirmationDialog()
    {
        var deleteRequested = false;
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new InventoryTagFormModel())
            .Add(component => component.OnDeleteRequested, () => deleteRequested = true));

        await cut.Find(".editor-modal__footer-danger .levelup-button--danger").ClickAsync();

        Assert.True(deleteRequested);
        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }

    [Fact]
    public void RendersAsASiblingShapeMatchingTheSharedEditorModalShell()
    {
        // Uses the exact same EditorModalShell markup contract as every other editor modal
        // (Transaction, Project, Habit, Task, To-Do) — .editor-modal-backdrop / .editor-modal —
        // with no Inventory-specific modal classes.
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new InventoryTagFormModel()));

        Assert.NotEmpty(cut.FindAll(".editor-modal-backdrop"));
        Assert.NotEmpty(cut.FindAll(".editor-modal"));
        Assert.Empty(cut.FindAll(".inventory-modal"));
        Assert.Empty(cut.FindAll(".inventory-modal-backdrop"));
    }
}
