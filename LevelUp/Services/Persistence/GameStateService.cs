using LevelUp.Domain;
using LevelUp.Services.Bosses;
using LevelUp.Services.Habits;
using LevelUp.Services.Milestones;
using LevelUp.Services.Projects;
using LevelUp.Services.Quests;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Persistence;

public sealed class GameStateService
{
    private readonly IGameDataStore dataStore;
    private readonly HabitService habitService;
    private readonly ProjectService projectService;
    private readonly QuestService questService;
    private readonly MilestoneService milestoneService;
    private readonly BossService bossService;
    private readonly CharacterModel character;

    public GameStateService(
        IGameDataStore dataStore,
        HabitService habitService,
        ProjectService projectService,
        QuestService questService,
        MilestoneService milestoneService,
        BossService bossService,
        CharacterModel character
    )
    {
        this.dataStore = dataStore;
        this.habitService = habitService;
        this.projectService = projectService;
        this.questService = questService;
        this.milestoneService = milestoneService;
        this.bossService = bossService;
        this.character = character;
    }

    public GameData CreateSnapshot()
    {
        return new GameData
        {
            Character = character,
            Habits = habitService.GetAllHabits().ToList(),
            Projects = projectService.GetAllProjects().ToList(),
            Quests = questService.GetAllQuests().ToList(),
            Milestones = milestoneService.GetAll().ToList(),
            Bosses = bossService.GetAll().ToList()
        };
    }

    public void Save() => dataStore.Save(CreateSnapshot());
}
