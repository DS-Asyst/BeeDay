using BeeDay.Application.Features.Habits.Commands;
using BeeDay.Application.Features.Habits.Handlers;
using BeeDay.Application.Features.Tasks.Commands;
using BeeDay.Application.Features.Tasks.Handlers;
using BeeDay.Application.Features.Tasks.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

public sealed class MultiUserIsolationTests
{
    [Fact]
    public async Task Repositories_ReturnOnlyTheAuthenticatedUsersOwnActivities()
    {
        var repository = new FakeUnitOfWork();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        AddHabit(repository, first.Id, Habit.Create("First habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));
        AddHabit(repository, second.Id, Habit.Create("Second habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));
        AddTask(repository, first.Id, RecurringTask.Create("First task", "", TaskRepeat.None));
        AddTask(repository, second.Id, RecurringTask.Create("Second task", "", TaskRepeat.None));

        var habits = await repository.Habits.ListAsync(first.Id, TestContext.Current.CancellationToken);
        var tasks = await repository.RecurringTasks.ListAsync(first.Id, TestContext.Current.CancellationToken);

        Assert.Equal("First habit", Assert.Single(habits).Title);
        Assert.Equal("First task", Assert.Single(tasks).Title);
        Assert.DoesNotContain(habits, item => item.UserId == second.Id);
    }

    [Fact]
    public async Task User_CannotModifyAnotherUsersHabit()
    {
        var repository = new FakeUnitOfWork();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        var habit = Habit.Create("Private habit", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        AddHabit(repository, second.Id, habit);

        var handler = new RegisterHabitPositiveCommandHandler(repository, new FakeCurrentUserContext(first.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(new RegisterHabitPositiveCommand(habit.Id), TestContext.Current.CancellationToken));
        Assert.Equal(0, habit.PositiveCount);
    }

    [Fact]
    public async Task ConcurrentSessions_CreateActivitiesForTheirOwnUsers()
    {
        var repository = new FakeUnitOfWork();
        var first = AddUser(repository, "First", "first@levelup.test");
        var second = AddUser(repository, "Second", "second@levelup.test");
        var firstHandler = new CreateTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(first.Id));
        var secondHandler = new CreateTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(second.Id));

        await firstHandler.Handle(new CreateTaskCommand(new SaveTaskRequest("First task", "", TaskRepeat.None)), TestContext.Current.CancellationToken);
        await secondHandler.Handle(new CreateTaskCommand(new SaveTaskRequest("Second task", "", TaskRepeat.None)), TestContext.Current.CancellationToken);

        Assert.Contains(repository.RecurringTasksData, task => task.Title == "First task" && task.UserId == first.Id);
        Assert.Contains(repository.RecurringTasksData, task => task.Title == "Second task" && task.UserId == second.Id);
    }

    private static User AddUser(FakeUnitOfWork repository, string name, string email)
    {
        var user = User.Create(name, email);
        repository.UsersData.Add(user);
        return user;
    }

    private static void AddHabit(FakeUnitOfWork repository, Guid userId, Habit habit)
    {
        habit.AssignOwner(userId);
        repository.HabitsData.Add(habit);
    }

    private static void AddTask(FakeUnitOfWork repository, Guid userId, RecurringTask task)
    {
        task.AssignOwner(userId);
        repository.RecurringTasksData.Add(task);
    }
}
