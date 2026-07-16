using LevelUp.Domain.Achievements;
using LevelUp.Domain.Attributes;
using LevelUp.Domain.Books;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Character;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Wallet;

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

    public static string For(BookStatus status) => status switch
    {
        BookStatus.Locked => "Bloqueado",
        BookStatus.Reading => "Em andamento",
        BookStatus.Completed => "Concluído",
        BookStatus.Archived => "Arquivado",
        _ => status.ToString()
    };

    public static string For(AttributeType value) => value switch
    {
        AttributeType.Strength => "Força",
        AttributeType.Intelligence => "Inteligência",
        AttributeType.Vitality => "Vitalidade",
        AttributeType.Agility => "Agilidade",
        AttributeType.Dexterity => "Destreza",
        AttributeType.Luck => "Sorte",
        _ => value.ToString()
    };

    public static string For(CharacterClass value) => value switch
    {
        CharacterClass.Warrior => "Guerreiro",
        CharacterClass.Mage => "Mago",
        CharacterClass.Hunter => "Caçador",
        CharacterClass.Priest => "Sacerdote",
        CharacterClass.Paladin => "Paladino",
        CharacterClass.Rogue => "Ladino",
        _ => value.ToString()
    };

    public static string For(CharacterRank value) => value switch
    {
        CharacterRank.Apprentice => "Aprendiz",
        CharacterRank.Adventurer => "Aventureiro",
        CharacterRank.Disciple => "Discípulo",
        CharacterRank.Adept => "Adepto",
        CharacterRank.Specialist => "Especialista",
        CharacterRank.Master => "Mestre",
        CharacterRank.Legend => "Lenda",
        _ => value.ToString()
    };

    public static string For(AchievementStatus value) => value switch
    {
        AchievementStatus.Locked => "Bloqueada",
        AchievementStatus.Unlocked => "Desbloqueada",
        _ => value.ToString()
    };

    public static string For(AchievementCategory value) => value switch
    {
        AchievementCategory.Project => "Projeto",
        AchievementCategory.Mission => "Missão",
        AchievementCategory.Training => "Treinamento",
        AchievementCategory.Reading => "Leitura",
        AchievementCategory.Wallet => "Carteira",
        AchievementCategory.General => "Geral",
        _ => value.ToString()
    };
}
