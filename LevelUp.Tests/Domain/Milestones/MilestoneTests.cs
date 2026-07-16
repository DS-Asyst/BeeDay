using LevelUp.Domain.Milestones;
using Xunit;

namespace LevelUp.Tests.Domain.Milestones;

public sealed class MilestoneTests
{
    [Fact]
    public void Configure_ShouldAssociateMilestoneWithProject()
    {
        Milestone milestone = new();

        milestone.Configure(
            projectId: 1,
            title: "Complete persistence layer",
            description: "Finish save and load workflows."
        );

        Assert.Equal(1, milestone.ProjectId);
        Assert.Equal(
            "Complete persistence layer",
            milestone.Title
        );
        Assert.Equal(
            MilestoneStatus.Created,
            milestone.Status
        );
    }

    [Fact]
    public void Configure_ShouldRejectInvalidProjectId()
    {
        Milestone milestone = new();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => milestone.Configure(
                projectId: 0,
                title: "Invalid milestone",
                description: string.Empty
            )
        );
    }

    [Fact]
    public void Activate_ShouldMoveCreatedMilestoneToActive()
    {
        Milestone milestone = CreateMilestone();

        milestone.Activate();

        Assert.Equal(
            MilestoneStatus.Active,
            milestone.Status
        );
        Assert.NotNull(milestone.ActivatedAt);
    }

    [Fact]
    public void Complete_ShouldMoveActiveMilestoneToCompleted()
    {
        Milestone milestone = CreateMilestone();

        milestone.Activate();
        milestone.Complete();

        Assert.Equal(
            MilestoneStatus.Completed,
            milestone.Status
        );
        Assert.NotNull(milestone.CompletedAt);
    }

    [Fact]
    public void Complete_ShouldRejectCreatedMilestone()
    {
        Milestone milestone = CreateMilestone();

        Assert.Throws<InvalidOperationException>(
            milestone.Complete
        );
    }

    [Fact]
    public void UpdateDetails_ShouldRejectArchivedMilestone()
    {
        Milestone milestone = CreateMilestone();

        milestone.Archive();

        Assert.Throws<InvalidOperationException>(
            () => milestone.UpdateDetails(
                "Updated title",
                "Updated description"
            )
        );
    }

    private static Milestone CreateMilestone()
    {
        Milestone milestone = new();

        milestone.Configure(
            projectId: 1,
            title: "Initial milestone",
            description: "Initial description."
        );

        return milestone;
    }
}
