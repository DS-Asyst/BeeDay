using LevelUp.Application.Features.Dashboard.Responses;
using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class ProjectContextFilterTests
{
    [Fact]
    public void MenuIsClosedByDefault()
    {
        using var context = new BunitContext();

        var cut = context.Render<ProjectContextFilter>();

        Assert.Empty(cut.FindAll(".project-context-filter__menu"));
        Assert.Equal("false", cut.Find(".project-context-filter__trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task RendersAllProjectsAndAvailableProjectsWhenOpened()
    {
        using var context = new BunitContext();
        var project = CreateProject("Project A");

        var cut = context.Render<ProjectContextFilter>(parameters => parameters
            .Add(component => component.Projects, [project]));

        await cut.Find(".project-context-filter__trigger").ClickAsync();

        var options = cut.FindAll(".project-context-filter__option");

        Assert.Equal(2, options.Count);
        Assert.Equal("All Projects", options[0].TextContent.Trim());
        Assert.Equal("Project A", options[1].TextContent.Trim());
    }

    [Fact]
    public async Task EmitsSelectedProjectAndClearsBackToAllProjects()
    {
        using var context = new BunitContext();
        var project = CreateProject("Project A");
        Guid? selectedProjectId = null;

        var cut = context.Render<ProjectContextFilter>(parameters => parameters
            .Add(component => component.Projects, [project])
            .Add(component => component.SelectedProjectIdChanged, (Guid? value) => selectedProjectId = value));

        await cut.Find(".project-context-filter__trigger").ClickAsync();
        await cut.FindAll(".project-context-filter__option")[1].ClickAsync();
        Assert.Equal(project.Id, selectedProjectId);

        await cut.Find(".project-context-filter__trigger").ClickAsync();
        await cut.Find(".project-context-filter__option").ClickAsync();
        Assert.Null(selectedProjectId);
    }

    [Fact]
    public async Task ClosesTheMenuAfterSelectingAnOption()
    {
        using var context = new BunitContext();
        var project = CreateProject("Project A");

        var cut = context.Render<ProjectContextFilter>(parameters => parameters
            .Add(component => component.Projects, [project]));

        await cut.Find(".project-context-filter__trigger").ClickAsync();
        await cut.Find(".project-context-filter__option").ClickAsync();

        Assert.Empty(cut.FindAll(".project-context-filter__menu"));
    }

    private static ProjectSummary CreateProject(string name) => new(
        Guid.NewGuid(),
        name,
        "Description",
        "#7A4FCB",
        Featured: false,
        Attribute: null,
        ExpectedDate: null,
        Archived: false,
        Status: ProjectStatus.Planned,
        ProgressPercentage: 0m,
        Todos: []);
}
