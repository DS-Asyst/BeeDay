using LevelUp.Domain;
using LevelUp.Domain.Tasks;
using LevelUp.Domain.Todos;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Persistence.Migrations;

public sealed class MigrationV6ToV7 : IGameDataMigration
{
    public int SourceVersion => 6; public int TargetVersion => 7;
    public void Apply(GameData gameData)
    {
        foreach (var quest in gameData.LegacyQuests)
        {
            if (quest.ProjectId is null)
            {
                var task = new TaskItem { Id = quest.Id };
                task.Configure(quest.Title, quest.Description, quest.AttributeType, TaskRecurrence.Daily, WeekDays.EveryDay);
                if (quest.Status == QuestStatus.Completed) task.Complete(quest.CompletedAt);
                gameData.Tasks.Add(task);
            }
            else
            {
                var todo = new ProjectTodo { Id = quest.Id };
                todo.Configure(quest.ProjectId.Value, quest.MilestoneId, quest.Title, quest.Description, quest.AttributeType);
                if (quest.Status is QuestStatus.Active or QuestStatus.Completed) todo.Activate();
                if (quest.Status == QuestStatus.Completed) todo.Complete();
                gameData.Todos.Add(todo);
            }
        }
        gameData.SchemaVersion = TargetVersion;
    }
}
