using BeeDay.Web.Components.Features.Projects.Components;
using BeeDay.Web.Components.Features.Projects.Models;

namespace BeeDay.Web.Tests.Components.Projects;

public sealed class ProjectEditorModalTests : BunitContext
{
    [Fact]
    public void OpenProjectAction_RendersOnlyWhenEditing()
    {
        var editingCut = Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true));

        Assert.Contains(editingCut.FindAll("button"), button => button.TextContent.Contains("Open Project", StringComparison.Ordinal));

        var creatingCut = Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel())
            .Add(component => component.IsEditing, false));

        Assert.DoesNotContain(creatingCut.FindAll("button"), button => button.TextContent.Contains("Open Project", StringComparison.Ordinal));
    }

    [Fact]
    public void OpenProjectAction_MatchesNewTagsVisualWeight()
    {
        // Compact + the comic border/shadow treatment, no icon — same size and shape family
        // as WalletTagManager's "New tag" button, not the larger plain pixel-button style.
        var cut = Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true));

        var button = cut.FindAll("button").First(element => element.TextContent.Contains("Open Project", StringComparison.Ordinal));

        Assert.Contains("beeday-button--compact", button.ClassList);
        Assert.Contains("beeday-button--comic", button.ClassList);
        Assert.Empty(button.QuerySelectorAll("svg"));
    }

    [Fact]
    public async Task ClickingOpenProject_InvokesOnOpenProject()
    {
        var invoked = false;
        var cut = Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true)
            .Add(component => component.OnOpenProject, () => invoked = true));

        var button = cut.FindAll("button").First(element => element.TextContent.Contains("Open Project", StringComparison.Ordinal));
        await button.ClickAsync();

        Assert.True(invoked);
    }
}
