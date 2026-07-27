using LevelUp.Domain.Entities;
using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.Dashboard;

public sealed class ProjectContextFilterTests
{
    [Fact]
    public void RendersAllProjectsAndAvailableProjects()
    {
        using var context = new BunitContext();
        var project = Project.Create("Project A", "Description");

        var cut = context.Render<ProjectContextFilter>(parameters => parameters
            .Add(component => component.Projects, [project]));

        var options = cut.FindAll("option");

        Assert.Equal(2, options.Count);
        Assert.Equal("All Projects", options[0].TextContent);
        Assert.Equal("Project A", options[1].TextContent);
    }

    [Fact]
    public async Task EmitsSelectedProjectAndClearsBackToAllProjects()
    {
        using var context = new BunitContext();
        var project = Project.Create("Project A", "Description");
        Guid? selectedProjectId = null;

        var cut = context.Render<ProjectContextFilter>(parameters => parameters
            .Add(component => component.Projects, [project])
            .Add(component => component.SelectedProjectIdChanged, (Guid? value) => selectedProjectId = value));

        await cut.Find("select").ChangeAsync(project.Id.ToString());
        Assert.Equal(project.Id, selectedProjectId);

        await cut.Find("select").ChangeAsync(string.Empty);
        Assert.Null(selectedProjectId);
    }
}
