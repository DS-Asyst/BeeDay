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
        ProjectStatus.Created => "Created",
        ProjectStatus.Active => "Active",
        ProjectStatus.Completed => "Completed",
        ProjectStatus.Archived => "Archived",
        _ => status.ToString()
    };

    public static string For(QuestStatus status) => status switch
    {
        QuestStatus.Created => "Created",
        QuestStatus.Active => "Active",
        QuestStatus.Completed => "Completed",
        QuestStatus.Archived => "Archived",
        _ => status.ToString()
    };

    public static string For(MilestoneStatus status) => status switch
    {
        MilestoneStatus.Locked => "Locked",
        MilestoneStatus.Created => "Created",
        MilestoneStatus.Active => "Active",
        MilestoneStatus.Completed => "Completed",
        MilestoneStatus.Archived => "Archived",
        _ => status.ToString()
    };

    public static string For(BossStatus status) => status switch
    {
        BossStatus.Locked => "Locked",
        BossStatus.Available => "Available",
        BossStatus.Defeated => "Defeated",
        BossStatus.Archived => "Archived",
        _ => status.ToString()
    };

    public static string For(BookStatus status) => status switch
    {
        BookStatus.Locked => "Locked",
        BookStatus.Reading => "In Progress",
        BookStatus.Completed => "Completed",
        BookStatus.Archived => "Archived",
        _ => status.ToString()
    };

    public static string For(AttributeType value) => value switch
    {
        AttributeType.Strength => "Strength",
        AttributeType.Intelligence => "Intelligence",
        AttributeType.Vitality => "Vitality",
        AttributeType.Agility => "Agility",
        AttributeType.Dexterity => "Dexterity",
        AttributeType.Luck => "Luck",
        _ => value.ToString()
    };

    public static string For(CharacterClass value) => value switch
    {
        CharacterClass.Warrior => "Warrior",
        CharacterClass.Mage => "Mage",
        CharacterClass.Hunter => "Hunter",
        CharacterClass.Priest => "Priest",
        CharacterClass.Paladin => "Paladin",
        CharacterClass.Rogue => "Rogue",
        _ => value.ToString()
    };

    public static string For(CharacterRank value) => value switch
    {
        CharacterRank.Apprentice => "Apprentice",
        CharacterRank.Adventurer => "Adventurer",
        CharacterRank.Disciple => "Disciple",
        CharacterRank.Adept => "Adept",
        CharacterRank.Specialist => "Specialist",
        CharacterRank.Master => "Master",
        CharacterRank.Legend => "Legend",
        _ => value.ToString()
    };

    public static string For(AchievementStatus value) => value switch
    {
        AchievementStatus.Locked => "Locked",
        AchievementStatus.Unlocked => "Unlocked",
        _ => value.ToString()
    };

    public static string For(AchievementCategory value) => value switch
    {
        AchievementCategory.Project => "Project",
        AchievementCategory.Mission => "Task",
        AchievementCategory.Training => "Habit",
        AchievementCategory.Reading => "Reading",
        AchievementCategory.Wallet => "Wallet",
        AchievementCategory.General => "General",
        _ => value.ToString()
    };
}
