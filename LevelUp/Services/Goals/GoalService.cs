using LevelUp.Application;
using LevelUp.Domain.Books;
using LevelUp.Domain.Goals;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Goals;

public sealed class GoalService
{
    private readonly List<Goal> goals = [];
    private int nextId = 1;

    public GoalService(IEnumerable<Goal>? goals = null)
    {
        if (goals is null) return;
        this.goals.AddRange(goals);
        if (this.goals.Count > 0) nextId = this.goals.Max(item => item.Id) + 1;
    }

    public Goal Create(string name, string description, GoalMetric metric, decimal targetValue)
    {
        Goal goal = new() { Id = nextId++ };
        goal.Configure(name, description, metric, targetValue);
        goals.Add(goal);
        return goal;
    }

    public IReadOnlyList<Goal> GetAll() => goals.AsReadOnly();
    public Goal? GetById(int id) => goals.FirstOrDefault(item => item.Id == id);

    public decimal GetCurrentValue(Goal goal, GameSession session)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(session);
        return goal.Metric switch
        {
            GoalMetric.CompletedQuests => session.Quests.GetAllQuests().Count(item => item.Status == QuestStatus.Completed),
            GoalMetric.CompletedTrainings => session.Habits.GetAllHabits().Sum(item => item.TimesCompleted),
            GoalMetric.CompletedProjects => session.Projects.GetAllProjects().Count(item => item.Status == ProjectStatus.Completed),
            GoalMetric.CompletedBooks => session.Books.GetAll().Count(item => item.Status == BookStatus.Completed),
            GoalMetric.PagesRead => session.Books.GetAll().SelectMany(item => item.ProgressHistory).Sum(item => item.PagesRead),
            GoalMetric.SavingsBalance => session.Wallet.Balance,
            _ => 0m
        };
    }

    public IReadOnlyList<Goal> EvaluateAll(GameSession session)
    {
        List<Goal> completed = [];
        foreach (Goal goal in goals.Where(item => item.Status == GoalStatus.Active))
        {
            if (goal.Evaluate(GetCurrentValue(goal, session))) completed.Add(goal);
        }
        return completed;
    }

    public bool Delete(int id)
    {
        Goal? goal = GetById(id);
        return goal is not null && goals.Remove(goal);
    }
}
