using LevelUp.Domain;
using LevelUp.Services.Habits;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class GameStateService
{
    private readonly SaveService saveService;
    private readonly HabitService habitService;
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly CharacterModel character;


    public GameStateService(
        SaveService saveService,
        HabitService habitService,
        ProjectService projectService,
        QuestService questService,
        CharacterModel character
    )
    {
        this.saveService = saveService;
        this.habitService = habitService;
        this.projectService = projectService;
        this.questService = questService;
        this.character = character;
    }

    public GameData CreateSnapshot()
    {
        return new GameData
        {
            Character = character,

            Habits = habitService
                .GetAllHabits()
                .ToList(),

            Projects = projectService
                .GetAllProjects()
                .ToList(),

            Quests = questService
                .GetAllQuests()
                .ToList()
        };
    }

    public void Save()
    {
        GameData gameData = CreateSnapshot();

        saveService.SaveGame(gameData);
    }


}
