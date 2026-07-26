using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Application.Features.Tasks.Handlers;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Application.Features.Todos.Handlers;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;

namespace LevelUp.Application.Tests;

public sealed class ExperienceRewardPipelineTests
{
    [Fact]
    public async Task Completing_task_twice_grants_experience_only_once()
    {
        var repository = CreateRepository();
        var task = RecurringTask.Create("Review pull request", null, TaskRepeat.None);
        repository.Data.AddTask(repository.User.Id, task);
        var handler = new ToggleTaskCommandHandler(repository, new UserContext(repository.User.Id), new ExperienceRewardService());

        await handler.Handle(new ToggleTaskCommand(task.Id), TestContext.Current.CancellationToken);
        await handler.Handle(new ToggleTaskCommand(task.Id), TestContext.Current.CancellationToken);
        await handler.Handle(new ToggleTaskCommand(task.Id), TestContext.Current.CancellationToken);

        ExperienceEntry entry = Assert.Single(repository.Character.Experience.Entries);
        Assert.Equal(5L, repository.Character.Experience.TotalExperience);
        Assert.Equal(ExperienceSourceType.Task, entry.Source.Type);
        Assert.Equal(task.Id, entry.Source.ReferenceId);
        Assert.Equal(ExperienceRewardType.Completion, entry.RewardType);
        Assert.Equal(repository.Character.Id, entry.CharacterId);
        Assert.Equal(0L, entry.ExperienceBefore);
        Assert.Equal(5L, entry.ExperienceAfter);
        Assert.Equal(1, entry.LevelBefore);
        Assert.Equal(1, entry.LevelAfter);
    }

    [Fact]
    public async Task Completing_last_todo_grants_todo_and_project_rewards_once()
    {
        var repository = CreateRepository();
        var project = Project.Create("Release Sprint 3.3", null);
        repository.Data.AddProject(repository.User.Id, project);
        var todo = Todo.Create(project.Id, "Validate pipeline", null, null);
        todo.AssignOwner(repository.User.Id);
        project.AddTodo(todo);
        var handler = new ToggleTodoCommandHandler(repository, new UserContext(repository.User.Id), new ExperienceRewardService());

        await handler.Handle(new ToggleTodoCommand(todo.Id), TestContext.Current.CancellationToken);
        await handler.Handle(new ToggleTodoCommand(todo.Id), TestContext.Current.CancellationToken);
        await handler.Handle(new ToggleTodoCommand(todo.Id), TestContext.Current.CancellationToken);

        Assert.Equal(27L, repository.Character.Experience.TotalExperience);
        Assert.Equal(2, repository.Character.Experience.Entries.Count);
        Assert.Single(repository.Character.Experience.Entries, item => item.Source.Type == ExperienceSourceType.Todo);
        Assert.Single(repository.Character.Experience.Entries, item => item.Source.Type == ExperienceSourceType.Project);
    }

    [Fact]
    public void Central_service_rejects_duplicate_reward_key()
    {
        var repository = CreateRepository();
        var service = new ExperienceRewardService();
        var sourceId = Guid.NewGuid();

        var first = service.Grant(repository.Data, repository.User.Id, ExperienceSourceType.Task, sourceId, ExperienceRewardType.Completion, "Chapter completed");
        var duplicate = service.Grant(repository.Data, repository.User.Id, ExperienceSourceType.Task, sourceId, ExperienceRewardType.Completion, "Chapter completed again");

        Assert.NotNull(first);
        Assert.Null(duplicate);
        Assert.Equal(5L, repository.Character.Experience.TotalExperience);
        Assert.Single(repository.Character.Experience.Entries);
    }

    [Fact]
    public void Reward_policy_centralizes_initial_balance()
    {
        var policy = new ExperienceRewardPolicy();

        Assert.Equal(1L, policy.GetReward(ExperienceSourceType.Habit));
        Assert.Equal(5L, policy.GetReward(ExperienceSourceType.Task));
        Assert.Equal(7L, policy.GetReward(ExperienceSourceType.Todo));
        Assert.Equal(20L, policy.GetReward(ExperienceSourceType.Project));
        Assert.Throws<LevelUp.Domain.Exceptions.DomainValidationException>(() => policy.GetReward(ExperienceSourceType.System));
    }

    [Fact]
    public async Task Positive_habit_grants_experience_for_each_distinct_occurrence()
    {
        var repository = CreateRepository();
        var habit = Habit.Create("Drink water", null, HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        repository.Data.AddHabit(repository.User.Id, habit);
        var handler = new LevelUp.Application.Features.Habits.Handlers.RegisterHabitPositiveCommandHandler(
            repository, new UserContext(repository.User.Id), new ExperienceRewardService());

        await handler.Handle(new LevelUp.Application.Features.Habits.Commands.RegisterHabitPositiveCommand(habit.Id), TestContext.Current.CancellationToken);
        await handler.Handle(new LevelUp.Application.Features.Habits.Commands.RegisterHabitPositiveCommand(habit.Id), TestContext.Current.CancellationToken);

        Assert.Equal(2L, repository.Character.Experience.TotalExperience);
        Assert.Equal(2, repository.Character.Experience.Entries.Count);
        Assert.All(repository.Character.Experience.Entries, entry => Assert.Equal(ExperienceSourceType.Habit, entry.SourceType));
        Assert.Equal(2, repository.Character.Experience.Entries.Select(entry => entry.SourceId).Distinct().Count());
    }

    [Theory]
    [InlineData(ExperienceSourceType.Reading)]
    [InlineData(ExperienceSourceType.Manual)]
    [InlineData(ExperienceSourceType.System)]
    public void Automatic_policy_rejects_sources_without_configured_rewards(ExperienceSourceType sourceType)
    {
        var policy = new ExperienceRewardPolicy();

        Assert.Throws<LevelUp.Domain.Exceptions.DomainValidationException>(() => policy.GetReward(sourceType));
    }

    private static TestRepository CreateRepository()
    {
        var repository = new TestRepository();
        repository.Data.AddUser(repository.User);
        repository.Data.SetCurrentUser(repository.User.Id);
        repository.Data.AddCharacter(repository.Character);
        return repository;
    }

    private sealed record UserContext(Guid Id) : ICurrentUserContext
    {
        public Guid? UserId => Id;
    }

    private sealed class TestRepository : ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();
        public User User { get; } = User.Create("Pipeline User", "pipeline@levelup.invalid");
        public Character Character { get; }

        public TestRepository() => Character = Character.Create(User.Id, "pipelinehero", CharacterClass.Warrior);

        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Data);
        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
