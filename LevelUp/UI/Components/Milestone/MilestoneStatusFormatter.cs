using LevelUp.Domain.Milestones;
using LevelUp.UI.Infrastructure.Themes;

namespace LevelUp.UI.Components.Milestone;

public static class MilestoneStatusFormatter
{
    public static string Format(MilestoneStatus status)
    {
        return status switch
        {
            MilestoneStatus.Locked => $"[{LevelUpTheme.MutedText}]× Locked[/]",
            MilestoneStatus.Created => $"[{LevelUpTheme.MutedText}]{UIIcons.Created} Created[/]",
            MilestoneStatus.Active => $"[{LevelUpTheme.Information}]{UIIcons.Active} Active[/]",
            MilestoneStatus.Completed => $"[{LevelUpTheme.Success}]{UIIcons.Completed} Completed[/]",
            MilestoneStatus.Archived => $"[{LevelUpTheme.MutedText}]{UIIcons.Archived} Archived[/]",
            _ => status.ToString()
        };
    }
}
