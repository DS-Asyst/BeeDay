using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Xunit;

namespace LevelUp.Domain.Tests;

public sealed class ProjectTests
{
    [Fact]
    public void SetStatus_Completed_SynchronizesCompletion()
    {
        var project = Project.Create("LevelUp", null, ProjectStatus.InProgress);
        project.SetStatus(ProjectStatus.Completed);
        Assert.True(project.Completed);
    }

    [Fact]
    public void ToggleCompletion_UsesProjectStatusInvariant()
    {
        var project = Project.Create("LevelUp", null, ProjectStatus.InProgress);
        project.ToggleCompletion();
        Assert.Equal(ProjectStatus.Completed, project.Status);
        Assert.True(project.Completed);
    }
}
