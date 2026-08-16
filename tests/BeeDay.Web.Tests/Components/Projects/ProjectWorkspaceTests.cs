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
        JSInterop.Mode = JSRuntimeMode.Loose;
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

    [Fact]
    public void UsesSharedProgressAndIconControlsWithLocalizedSemantics()
    {
        var project = CreateProject("Kitchen remodel", string.Empty, ProjectStatus.InProgress, [], progress: 42m);
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)));

        var progress = cut.Find("[role='progressbar']");
        Assert.Equal("Project progress 42%", progress.GetAttribute("aria-label"));
        Assert.Equal("42%", progress.GetAttribute("aria-valuetext"));
        Assert.Contains("beeday-icon-toggle", cut.Find(".project-workspace__close").ClassList);
        Assert.Contains("beeday-icon-toggle", cut.Find(".project-workspace__add").ClassList);
    }

    [Fact]
    public void PreservesTheAuthoritativeTodoSequenceInsteadOfApplyingAVisualSort()
    {
        var first = CreateTodo("Completed but manually first", completed: true, dueDate: new DateOnly(2026, 12, 31));
        var second = CreateTodo("Active but manually second", completed: false, dueDate: new DateOnly(2026, 1, 1));
        var project = CreateProject("Kitchen remodel", string.Empty, ProjectStatus.InProgress, [first, second]);
        var cut = Render<ProjectWorkspace>(parameters => parameters.Add(component => component.Project, project));

        Assert.Equal(
            [first.Title, second.Title],
            cut.FindAll(".project-workspace__todo strong").Select(element => element.TextContent));
    }

    [Fact]
    public void EscapeClosesTheWorkspaceThroughTheSharedDialogLifecycle()
    {
        var closed = false;
        var project = CreateProject("Kitchen remodel", string.Empty, ProjectStatus.InProgress, []);
        var cut = Render<ProjectWorkspace>(parameters => parameters
            .Add(component => component.Project, project)
            .Add(component => component.OnClose, () => closed = true));

        cut.Find(".project-workspace").KeyDown("Escape");

        Assert.True(closed);
    }

    private static ProjectSummary CreateProject(string name, string description, ProjectStatus status, IReadOnlyList<TodoSummary> todos, decimal progress = 0m) => new(
        Guid.NewGuid(), name, description, "#8056C7", Featured: false, Attribute: null,
        ExpectedDate: null, Archived: false, Status: status, ProgressPercentage: progress, Todos: todos);

    private static TodoSummary CreateTodo(string title, bool completed = false, DateOnly? dueDate = null) => new(
        Guid.NewGuid(), title, string.Empty, Guid.NewGuid(), Featured: false, DueDate: dueDate,
        Attribute: null, Completed: completed, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
}
