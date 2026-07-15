using LevelUp.Domain.Projects;
using LevelUp.UI.Infrastructure.Themes;

namespace LevelUp.UI.Components.Project;

public static class ProjectStatusFormatter
{
    public static string Format(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Created =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Created} Created[/]",

            ProjectStatus.Active =>
                $"[{LevelUpTheme.Information}]" +
                $"{UIIcons.Active} Active[/]",

            ProjectStatus.Completed =>
                $"[{LevelUpTheme.Success}]" +
                $"{UIIcons.Completed} Completed[/]",

            ProjectStatus.Archived =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Archived} Archived[/]",

            _ => status.ToString()
        };
    }
}
