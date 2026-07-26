using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Application.Features.Tasks.Handlers;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Application.Features.Todos.Handlers;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;

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

        var transaction = Assert.Single(repository.Character.Experience.Transactions);
        Assert.Equal(20L, repository.Character.Experience.TotalExperience);
        Assert.Equal(ExperienceSourceType.Task, transaction.Source.Type);
        Assert.Equal(task.Id, transaction.Source.ReferenceId);
        Assert.Equal(ExperienceRewardType.Completion, transaction.RewardType);
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

        Assert.Equal(75L, repository.Character.Experience.TotalExperience);
        Assert.Equal(2, repository.Character.Experience.Transactions.Count);
        Assert.Single(repository.Character.Experience.Transactions, item => item.Source.Type == ExperienceSourceType.Todo);
        Assert.Single(repository.Character.Experience.Transactions, item => item.Source.Type == ExperienceSourceType.Project);
    }

    [Fact]
    public void Central_service_rejects_duplicate_reward_key()
    {
        var repository = CreateRepository();
        var service = new ExperienceRewardService();
        var sourceId = Guid.NewGuid();

        var first = service.Grant(repository.Data, repository.User.Id, ExperienceSourceType.Reading, sourceId, ExperienceRewardType.Completion, "Chapter completed");
        var duplicate = service.Grant(repository.Data, repository.User.Id, ExperienceSourceType.Reading, sourceId, ExperienceRewardType.Completion, "Chapter completed again");

        Assert.NotNull(first);
        Assert.Null(duplicate);
        Assert.Equal(10L, repository.Character.Experience.TotalExperience);
        Assert.Single(repository.Character.Experience.Transactions);
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
