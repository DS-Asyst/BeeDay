using LevelUp.Domain.Milestones;
using LevelUp.UI.Infrastructure.Themes;

namespace LevelUp.UI.Components.Milestone;

public static class MilestoneStatusFormatter
{
    public static string Format(MilestoneStatus status)
    {
        return status switch
        {
            MilestoneStatus.Locked => $"[{LevelUpTheme.MutedText}]× Bloqueado[/]",
            MilestoneStatus.Created => $"[{LevelUpTheme.MutedText}]{UIIcons.Created} Criado[/]",
            MilestoneStatus.Active => $"[{LevelUpTheme.Information}]{UIIcons.Active} Ativo[/]",
            MilestoneStatus.Completed => $"[{LevelUpTheme.Success}]{UIIcons.Completed} Concluído[/]",
            MilestoneStatus.Archived => $"[{LevelUpTheme.MutedText}]{UIIcons.Archived} Arquivado[/]",
            _ => status.ToString()
        };
    }
}
