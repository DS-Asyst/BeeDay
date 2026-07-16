using LevelUp.Domain.Attributes;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.UI.Infrastructure;

public static class DisplayText
{
    public static string For(ProjectStatus status) => status switch
    {
        ProjectStatus.Created => "Criado",
        ProjectStatus.Active => "Ativo",
        ProjectStatus.Completed => "Concluído",
        ProjectStatus.Archived => "Arquivado",
        _ => status.ToString()
    };

    public static string For(QuestStatus status) => status switch
    {
        QuestStatus.Created => "Criada",
        QuestStatus.Active => "Ativa",
        QuestStatus.Completed => "Concluída",
        QuestStatus.Archived => "Arquivada",
        _ => status.ToString()
    };

    public static string For(MilestoneStatus status) => status switch
    {
        MilestoneStatus.Locked => "Bloqueado",
        MilestoneStatus.Created => "Criado",
        MilestoneStatus.Active => "Ativo",
        MilestoneStatus.Completed => "Concluído",
        MilestoneStatus.Archived => "Arquivado",
        _ => status.ToString()
    };

    public static string For(BossStatus status) => status switch
    {
        BossStatus.Locked => "Bloqueado",
        BossStatus.Available => "Disponível",
        BossStatus.Defeated => "Derrotado",
        BossStatus.Archived => "Arquivado",
        _ => status.ToString()
    };

    public static string For(AttributeType attributeType) => attributeType switch
    {
        AttributeType.Strength => "Força",
        AttributeType.Intelligence => "Inteligência",
        AttributeType.Vitality => "Vitalidade",
        AttributeType.Agility => "Agilidade",
        AttributeType.Dexterity => "Destreza",
        AttributeType.Luck => "Sorte",
        _ => attributeType.ToString()
    };
}
