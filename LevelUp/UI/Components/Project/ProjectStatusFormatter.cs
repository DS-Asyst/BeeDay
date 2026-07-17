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
                $"{UIIcons.Created} Criado[/]",

            ProjectStatus.Active =>
                $"[{LevelUpTheme.Information}]" +
                $"{UIIcons.Active} Ativo[/]",

            ProjectStatus.Completed =>
                $"[{LevelUpTheme.Success}]" +
                $"{UIIcons.Completed} Concluído[/]",

            ProjectStatus.Archived =>
                $"[{LevelUpTheme.MutedText}]" +
                $"{UIIcons.Archived} Arquivado[/]",

            _ => status.ToString()
        };
    }
}
