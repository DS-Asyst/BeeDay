using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Tasks.Components;
using BeeDay.Web.Components.Features.Tasks.Models;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Tasks;

public sealed class TaskEditorModalTests : BunitContext
{
    public TaskEditorModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Theory]
    [InlineData("en-US", true, "Edit Task", "Save")]
    [InlineData("en-US", false, "Create Task", "Create")]
    [InlineData("pt-BR", true, "Editar tarefa", "Salvar")]
    [InlineData("pt-BR", false, "Criar tarefa", "Criar")]
    public void TitleAndSubmitLabel_FollowCultureAndEditingState(string culture, bool isEditing, string expectedTitle, string expectedSubmit)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, new TaskEditorModel())
            .Add(component => component.IsEditing, isEditing)));

        Assert.Contains(expectedTitle, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(expectedSubmit, cut.Find(".editor-modal__header-save").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_FieldLabelsAndSummaryAreLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, new TaskEditorModel())
            .Add(component => component.IsEditing, false)));

        Assert.Contains("Título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Adicionar um título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Notas", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Tarefa", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Uma atividade recorrente ou repetível.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Repetição", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AllRepeatValues_RenderWithEnglishLabels_UnderEnglishCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, new TaskEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#task-repeat").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<TaskRepeat>().Length, options.Count);
        Assert.Equal(["None", "Daily", "Weekly", "Monthly"], options);
    }

    [Fact]
    public void AllRepeatValues_RenderWithPortugueseLabels_UnderPortugueseCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, new TaskEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#task-repeat").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<TaskRepeat>().Length, options.Count);
        Assert.Equal(["Nenhuma", "Diariamente", "Semanalmente", "Mensalmente"], options);
    }

    [Fact]
    public void TaskTitleAndNotes_TypedByTheUser_AreNeverLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, new TaskEditorModel { Title = "Comprar presente da Ana", Description = "Loja perto de casa" })
            .Add(component => component.IsEditing, true)));

        Assert.Contains("Comprar presente da Ana", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Loja perto de casa", cut.Markup, StringComparison.Ordinal);
    }

    // EPIC 30 Sprint 30.13: mirrors HabitEditorModalTests.Save_PassesTheEditedFieldsToOnSave (added
    // Sprint 30.12) — no prior test proved OnSave actually receives the edited Title/Description/
    // Repeat, only that the callback exists and submit-label text is culture-correct.
    [Fact]
    public async Task Save_PassesTheEditedFieldsToOnSave()
    {
        TaskEditorModel? saved = null;
        var model = new TaskEditorModel { Title = "Original", Description = "Original notes" };

        var cut = Render<TaskEditorModal>(parameters => parameters
            .Add(component => component.Model, model)
            .Add(component => component.IsEditing, true)
            .Add(component => component.OnSave, EventCallback.Factory.Create<TaskEditorModel>(this, m => saved = m)));

        cut.Find("#task-title").Change("Renew passport");
        cut.Find("#task-notes").Change("Bring old passport and photo");
        cut.Find("#task-repeat").Change(nameof(TaskRepeat.Weekly));
        await cut.Find(".editor-modal__header-save").ClickAsync();

        Assert.NotNull(saved);
        Assert.Equal("Renew passport", saved.Title);
        Assert.Equal("Bring old passport and photo", saved.Description);
        Assert.Equal(TaskRepeat.Weekly, saved.Repeat);
    }

    [Fact]
    public void UnderPortugueseUiCulture_DeleteConfirmationIsLocalized()
    {
        BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
        {
            var cut = Render<TaskEditorModal>(parameters => parameters
                .Add(component => component.Model, new TaskEditorModel { Title = "Comprar presente da Ana" })
                .Add(component => component.IsEditing, true));

            cut.Find(".editor-modal__footer-danger .beeday-button--danger").Click();

            var dialog = cut.Find("[role='alertdialog']");
            Assert.Contains("Excluir tarefa", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Tem certeza de que deseja excluir esta tarefa?", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Esta ação não pode ser desfeita.", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Todas as informações serão removidas permanentemente.", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Cancelar", dialog.TextContent, StringComparison.Ordinal);
        });
    }
}
