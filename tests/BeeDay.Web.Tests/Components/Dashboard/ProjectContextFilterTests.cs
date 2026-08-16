using System.Globalization;
using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Dashboard;

public sealed class ProjectContextFilterTests
{
    [Fact]
    public void MenuIsClosedByDefault()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = context.Render<ProjectContextFilter>();

        Assert.Empty(cut.FindAll(".project-context-filter__menu"));
        Assert.Equal("false", cut.Find(".project-context-filter__trigger").GetAttribute("aria-expanded"));
    }

    [Fact]
    public async Task RendersAllProjectsAndAvailableProjectsWhenOpened()
    {
        var restore = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
        try
        {
            using var context = new BunitContext().WithLocalization();
            var project = CreateProject("Project A");

            var cut = context.Render<ProjectContextFilter>(parameters => parameters
                .Add(component => component.Projects, [project]));

            await cut.Find(".project-context-filter__trigger").ClickAsync();

            var options = cut.FindAll(".project-context-filter__option");

            Assert.Equal(2, options.Count);
            Assert.Equal("All Projects", options[0].TextContent.Trim());
            Assert.Equal("Project A", options[1].TextContent.Trim());
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }

    [Fact]
    public async Task EmitsSelectedProjectAndClearsBackToAllProjects()
    {
        using var context = new BunitContext().WithLocalization();
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
        using var context = new BunitContext().WithLocalization();
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
