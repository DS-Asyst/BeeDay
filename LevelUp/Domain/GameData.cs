using LevelUp.Domain.Habits;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Domain;

public class GameData
{
    public CharacterModel Character { get; set; } = new();

    public List<Habit> Habits { get; set; } = [];

    public List<Project> Projects { get; set; } = [];

    public List<Quest> Quests { get; set; } = [];
}