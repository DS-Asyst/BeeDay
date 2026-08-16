using BeeDay.Web.Components.DesignSystem.Modals;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class EditorModalShellTests : BunitContext
{
    public EditorModalShellTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersExactTitleCaseButtonText_NotForcedUppercase()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Habit")
            .Add(component => component.TitleId, "habit-editor-title")
            .Add(component => component.SubmitLabel, "Save")
            .Add(component => component.ShowDelete, true)));

        Assert.Contains("Save", cut.Find(".editor-modal__header-save").TextContent);
        Assert.Contains("Delete", cut.Find(".editor-modal__footer-danger .beeday-button--danger").TextContent);
        Assert.Contains("Cancel", cut.Find(".editor-modal__cancel-action").TextContent);

        var dialog = cut.Find("[role='dialog']");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("false", dialog.GetAttribute("aria-busy"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.DoesNotContain("beeday-button--compact", cut.Find(".editor-modal__delete-action").ClassList);
    }

    [Fact]
    public void UnderEnglishUiCulture_DefaultsUnsetSubmitLabelAndFixedButtonsToEnglish()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Habit")
            .Add(component => component.TitleId, "habit-editor-title")
            .Add(component => component.ShowDelete, true)));

        Assert.Equal("SAVE", cut.Find(".editor-modal__header-save").TextContent.Trim());
        Assert.Equal("Delete", cut.Find(".editor-modal__footer-danger .beeday-button--danger").TextContent.Trim());
        Assert.Equal("Cancel", cut.Find(".editor-modal__cancel-action").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_DefaultsUnsetSubmitLabelAndFixedButtonsToPortuguese()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<EditorModalShell>(parameters => parameters
            .Add(component => component.Model, new object())
            .Add(component => component.Title, "Edit Habit")
            .Add(component => component.TitleId, "habit-editor-title")
            .Add(component => component.ShowDelete, true)));

        Assert.Equal("SALVAR", cut.Find(".editor-modal__header-save").TextContent.Trim());
        Assert.Equal("Excluir", cut.Find(".editor-modal__footer-danger .beeday-button--danger").TextContent.Trim());
        Assert.Equal("Cancelar", cut.Find(".editor-modal__cancel-action").TextContent.Trim());
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
        Assert.Empty(cut.Find(".editor-modal__footer-danger .beeday-button--danger").QuerySelectorAll("svg"));
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
