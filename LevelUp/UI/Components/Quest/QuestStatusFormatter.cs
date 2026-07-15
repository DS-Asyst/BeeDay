using LevelUp.Domain.Quests;
using LevelUp.UI.Infrastructure.Themes;

namespace LevelUp.UI.Components.Quest;

public static class QuestStatusFormatter
{
    public static string Format(QuestStatus status)
    {
        return status switch
        {
            QuestStatus.Created =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Created} Created[/]",

            QuestStatus.Active =>
                $"[{LevelUpTheme.Information}]" +
                $"{UIIcons.Active} Active[/]",

            QuestStatus.Completed =>
                $"[{LevelUpTheme.Success}]" +
                $"{UIIcons.Completed} Completed[/]",

            QuestStatus.Archived =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Archived} Archived[/]",

            _ => status.ToString()
        };
    }
}
