using BeeDay.Web.Components.DesignSystem.Modals;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class EditorModalShellTests : BunitContext
{
    [Fact]
    public void RendersExactTitleCaseButtonText_NotForcedUppercase()
    {
        var cut = Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Habit")
            .Add(component => component.TitleId, "habit-editor-title")
            .Add(component => component.SubmitLabel, "Save")
            .Add(component => component.ShowDelete, true));

        Assert.Contains("Save", cut.Find(".editor-modal__header-save").TextContent);
        Assert.Contains("Delete", cut.Find(".editor-modal__footer-danger .levelup-button--danger").TextContent);
        Assert.Contains("Cancel", cut.Find(".editor-modal__cancel-action").TextContent);
    }

    [Fact]
    public void CancelAndDeleteButtonsRenderNoIcon()
    {
        var cut = Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Habit")
            .Add(component => component.TitleId, "habit-editor-title")
            .Add(component => component.SubmitLabel, "Save")
            .Add(component => component.ShowDelete, true));

        Assert.Empty(cut.Find(".editor-modal__cancel-action").QuerySelectorAll("svg"));
        Assert.Empty(cut.Find(".editor-modal__footer-danger .levelup-button--danger").QuerySelectorAll("svg"));
    }

    [Fact]
    public void SecondaryAction_RendersWhenProvided()
    {
        var cut = Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Project")
            .Add(component => component.TitleId, "project-editor-title")
            .Add(component => component.SubmitLabel, "Save")
            .Add(component => component.SecondaryAction, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "test-secondary-action");
                builder.AddContent(2, "Open Project");
                builder.CloseElement();
            })));

        Assert.NotEmpty(cut.FindAll(".test-secondary-action"));
        Assert.Contains("Open Project", cut.Find(".test-secondary-action").TextContent);
    }
}
