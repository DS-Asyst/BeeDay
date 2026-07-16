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
                $"{UIIcons.Created} Criada[/]",

            QuestStatus.Active =>
                $"[{LevelUpTheme.Information}]" +
                $"{UIIcons.Active} Ativa[/]",

            QuestStatus.Completed =>
                $"[{LevelUpTheme.Success}]" +
                $"{UIIcons.Completed} Concluída[/]",

            QuestStatus.Archived =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Archived} Arquivada[/]",

            _ => status.ToString()
        };
    }
}
