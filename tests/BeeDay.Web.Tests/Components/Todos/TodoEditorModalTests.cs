using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Todos.Components;
using BeeDay.Web.Components.Features.Todos.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Todos;

public sealed class TodoEditorModalTests : BunitContext
{
    private static readonly IReadOnlyList<ProjectSummary> Projects =
    [
        new ProjectSummary(Guid.NewGuid(), "Reforma da cozinha", string.Empty, "#000000", false, null, null, false, ProjectStatus.InProgress, 0, [])
    ];

    public TodoEditorModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

    [Theory]
    [InlineData("en-US", true, "Edit To-Do", "Save")]
    [InlineData("en-US", false, "Create To-Do", "Create")]
    [InlineData("pt-BR", true, "Editar pendência", "Salvar")]
    [InlineData("pt-BR", false, "Criar pendência", "Criar")]
    public void TitleAndSubmitLabel_FollowCultureAndEditingState(string culture, bool isEditing, string expectedTitle, string expectedSubmit)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<TodoEditorModal>(parameters => parameters
            .Add(component => component.Model, new TodoEditorModel())
            .Add(component => component.Projects, Projects)
            .Add(component => component.IsEditing, isEditing)));

        Assert.Contains(expectedTitle, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(expectedSubmit, cut.Find(".editor-modal__header-save").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_FieldLabelsActionsAndSummaryAreLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TodoEditorModal>(parameters => parameters
            .Add(component => component.Model, new TodoEditorModel())
            .Add(component => component.Projects, Projects)
            .Add(component => component.IsEditing, false)));

        Assert.Contains("Título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Adicionar um título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Notas", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Pendência", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Uma ação que pertence a exatamente um projeto.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Projeto", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Selecione um projeto", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Data de vencimento", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void TodoTitleNotesAndProjectName_TypedByTheUser_AreNeverLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TodoEditorModal>(parameters => parameters
            .Add(component => component.Model, new TodoEditorModel { Title = "Comprar presente da Ana", Description = "Loja perto de casa" })
            .Add(component => component.Projects, Projects)
            .Add(component => component.IsEditing, true)));

        Assert.Contains("Comprar presente da Ana", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Loja perto de casa", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Reforma da cozinha", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void DueDateInput_StaysIsoFormatted_RegardlessOfCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TodoEditorModal>(parameters => parameters
            .Add(component => component.Model, new TodoEditorModel { DueDate = new DateTime(2026, 3, 5) })
            .Add(component => component.Projects, Projects)
            .Add(component => component.IsEditing, true)));

        var input = cut.Find("#todo-due-date");
        Assert.Equal("2026-03-05", input.GetAttribute("value"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_DeleteConfirmationIsLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<TodoEditorModal>(parameters => parameters
            .Add(component => component.Model, new TodoEditorModel { Title = "Comprar presente da Ana" })
            .Add(component => component.Projects, Projects)
            .Add(component => component.IsEditing, true)));

        cut.Find(".editor-modal__footer-danger .beeday-button--danger").Click();

        var dialog = cut.Find("[role='alertdialog']");
        Assert.Contains("Excluir pendência", dialog.TextContent, StringComparison.Ordinal);
        Assert.Contains("Tem certeza de que deseja excluir esta pendência?", dialog.TextContent, StringComparison.Ordinal);
        Assert.Contains("Esta ação não pode ser desfeita.", dialog.TextContent, StringComparison.Ordinal);
        Assert.Contains("Todas as informações serão removidas permanentemente.", dialog.TextContent, StringComparison.Ordinal);
        Assert.Contains("Cancelar", dialog.TextContent, StringComparison.Ordinal);
    }
}
