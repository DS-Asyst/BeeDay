using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Projects.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Projects;

public sealed class ProjectWorkspaceTests : BunitContext
{
    public ProjectWorkspaceTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishChromeAndPreservesUserContent()
    {
        var project = CreateProject("Kitchen remodel", "Full renovation", ProjectStatus.Planned, []);

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        Assert.Contains("Project", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Status", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Progress", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("To-Dos", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Break this project into clear, executable steps.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("No To-Dos yet", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Add the first step to move this project from Planned to In Progress.", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("Close project", cut.Find(".project-workspace__close").GetAttribute("aria-label"));

        // User content untouched.
        Assert.Contains("Kitchen remodel", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Full renovation", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseChromeAndPreservesUserContent()
    {
        var project = CreateProject("Reforma da cozinha", "Reforma completa", ProjectStatus.Planned, []);

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        Assert.Contains("Projeto", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Progresso", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Pendências", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Divida este projeto em etapas claras e executáveis.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Nenhuma pendência ainda", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Adicione o primeiro passo para mover este projeto de Planejado para Em andamento.", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("Fechar projeto", cut.Find(".project-workspace__close").GetAttribute("aria-label"));

        // User content untouched — not translated even though it happens to contain Portuguese words.
        Assert.Contains("Reforma da cozinha", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Reforma completa", cut.Markup, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(ProjectStatus.Planned, "en-US", "Planned")]
    [InlineData(ProjectStatus.InProgress, "en-US", "In Progress")]
    [InlineData(ProjectStatus.Completed, "en-US", "Completed")]
    [InlineData(ProjectStatus.Planned, "pt-BR", "Planejado")]
    [InlineData(ProjectStatus.InProgress, "pt-BR", "Em andamento")]
    [InlineData(ProjectStatus.Completed, "pt-BR", "Concluído")]
    public void EveryDefinedProjectStatus_HasALocalizedLabel_NeverTheRawEnumName(ProjectStatus status, string culture, string expectedLabel)
    {
        var project = CreateProject("Kitchen remodel", string.Empty, status, []);

        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        var statusValue = cut.Find(".project-workspace__summary div:first-child strong");
        Assert.Equal(expectedLabel, statusValue.TextContent);
    }

    [Fact]
    public void AddTodoButton_UnderPortugueseUiCulture_ExposesLocalizedAccessibleName()
    {
        var project = CreateProject("Kitchen remodel", string.Empty, ProjectStatus.Planned, []);

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        var addButton = cut.Find(".project-workspace__add");
        Assert.Equal("Adicionar pendência", addButton.GetAttribute("aria-label"));
        Assert.Equal("Adicionar pendência", addButton.GetAttribute("title"));
    }

    [Fact]
    public void TodoTitles_AreNeverLocalized()
    {
        var todo = CreateTodo("Escolher piso");
        var project = CreateProject("Kitchen remodel", string.Empty, ProjectStatus.InProgress, [todo]);

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        Assert.Contains("Escolher piso", cut.Markup, StringComparison.Ordinal);
        Assert.Equal("true", cut.Find(".project-workspace__list-toggle").GetAttribute("aria-expanded"));
    }

    private static ProjectSummary CreateProject(string name, string description, ProjectStatus status, IReadOnlyList<TodoSummary> todos) => new(
        Guid.NewGuid(), name, description, "#8056C7", Featured: false, Attribute: null,
        ExpectedDate: null, Archived: false, Status: status, ProgressPercentage: 0m, Todos: todos);

    private static TodoSummary CreateTodo(string title) => new(
        Guid.NewGuid(), title, string.Empty, Guid.NewGuid(), Featured: false, DueDate: null,
        Attribute: null, Completed: false, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
