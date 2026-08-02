using LevelUp.Application.Features.Dashboard.Handlers;
using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Application.Features.Tasks.Handlers;
using LevelUp.Application.Features.Tasks.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class MultiUserIsolationTests
{
    [Fact]
    public async Task Dashboard_ReturnsOnlyAuthenticatedUsersDailyData()
    {
        var repository = new FakeLevelUpRepository();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        repository.Data.AddHabit(first.Id, Habit.Create("First habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));
        repository.Data.AddHabit(second.Id, Habit.Create("Second habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));
        repository.Data.AddTask(first.Id, RecurringTask.Create("First task", "", TaskRepeat.None));
        repository.Data.AddTask(second.Id, RecurringTask.Create("Second task", "", TaskRepeat.None));

        var handler = new GetLevelUpQueryHandler(repository, new FakeApplicationCache(), new FakeCurrentUserContext(first.Id));
        var response = await handler.Handle(new GetLevelUpQuery(), TestContext.Current.CancellationToken);

        Assert.Equal(first.Id, response.Data.CurrentUserId);
        Assert.Equal("First habit", Assert.Single(response.Data.Habits).Title);
        Assert.Equal("First task", Assert.Single(response.Data.Tasks).Title);
        Assert.DoesNotContain(response.Data.Habits, item => item.UserId == second.Id);
    }

    [Fact]
    public async Task User_CannotModifyAnotherUsersHabit()
    {
        var repository = new FakeLevelUpRepository();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        var habit = Habit.Create("Private habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        repository.Data.AddHabit(second.Id, habit);

        var handler = new RegisterHabitPositiveCommandHandler(repository, new FakeCurrentUserContext(first.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(new RegisterHabitPositiveCommand(habit.Id), TestContext.Current.CancellationToken));
        Assert.Equal(0, habit.PositiveCount);
    }

    [Fact]
    public async Task ConcurrentSessions_CreateActivitiesForTheirOwnUsers()
    {
        var repository = new FakeLevelUpRepository();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        var firstHandler = new CreateTaskCommandHandler(repository, new FakeCurrentUserContext(first.Id));
        var secondHandler = new CreateTaskCommandHandler(repository, new FakeCurrentUserContext(second.Id));

        await firstHandler.Handle(new CreateTaskCommand(new SaveTaskRequest("First task", "", TaskRepeat.None)), TestContext.Current.CancellationToken);
        await secondHandler.Handle(new CreateTaskCommand(new SaveTaskRequest("Second task", "", TaskRepeat.None)), TestContext.Current.CancellationToken);

        Assert.Contains(repository.Data.Tasks, task => task.Title == "First task" && task.UserId == first.Id);
        Assert.Contains(repository.Data.Tasks, task => task.Title == "Second task" && task.UserId == second.Id);
    }

    private static User AddUser(FakeLevelUpRepository repository, string name, string email)
    {
        var user = User.Create(name, email);
        repository.Data.AddUser(user);
        return user;
    }
}
