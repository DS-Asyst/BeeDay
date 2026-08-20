using BeeDay.Application.Features.Habits.Commands;
using BeeDay.Application.Features.Habits.Handlers;
using BeeDay.Application.Features.Habits.Requests;
using BeeDay.Application.Features.Tasks.Commands;
using BeeDay.Application.Features.Tasks.Handlers;
using BeeDay.Application.Features.Tasks.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

public sealed class HabitTaskManagementHandlersTests
{
    [Fact]
    public async Task UpdateHabit_ChangesFieldsForTheOwningUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var habit = Habit.Create("Original", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(user.Id);
        repository.HabitsData.Add(habit);

        var handler = new UpdateHabitCommandHandler(repository.Habits, new FakeCurrentUserContext(user.Id));
        await handler.Handle(
            new UpdateHabitCommand(habit.Id, new SaveHabitRequest("Renamed", "Updated", HabitDirection.Both, HabitDifficulty.Hard, HabitResetCounter.Weekly)),
            TestContext.Current.CancellationToken);

        Assert.Equal("Renamed", habit.Title);
        Assert.Equal(HabitDirection.Both, habit.Direction);
        Assert.Equal(HabitDifficulty.Hard, habit.Difficulty);
    }

    [Fact]
    public async Task UpdateHabit_RejectsAnotherUsersHabit()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var habit = Habit.Create("Private", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(other.Id);
        repository.HabitsData.Add(habit);

        var handler = new UpdateHabitCommandHandler(repository.Habits, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateHabitCommand(habit.Id, new SaveHabitRequest("Hijacked", "", HabitDirection.Both, HabitDifficulty.Hard, HabitResetCounter.Weekly)),
            TestContext.Current.CancellationToken));
        Assert.Equal("Private", habit.Title);
    }

    [Fact]
    public async Task RegisterHabitNegative_IncrementsNegativeCountForTheOwningUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var habit = Habit.Create("Snacking", "", HabitDirection.Negative, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(user.Id);
        repository.HabitsData.Add(habit);

        var handler = new RegisterHabitNegativeCommandHandler(repository.Habits, new FakeCurrentUserContext(user.Id));
        await handler.Handle(new RegisterHabitNegativeCommand(habit.Id), TestContext.Current.CancellationToken);

        Assert.Equal(1, habit.NegativeCount);
    }

    [Fact]
    public async Task DeleteHabit_RemovesTheOwningUsersHabit()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var habit = Habit.Create("Disposable", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(user.Id);
        repository.HabitsData.Add(habit);

        var handler = new DeleteHabitCommandHandler(repository.Habits, new FakeCurrentUserContext(user.Id));
        await handler.Handle(new DeleteHabitCommand(habit.Id), TestContext.Current.CancellationToken);

        Assert.Empty(repository.HabitsData);
    }

    [Fact]
    public async Task DeleteHabit_RejectsAnotherUsersHabit()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var habit = Habit.Create("Private", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily);
        habit.AssignOwner(other.Id);
        repository.HabitsData.Add(habit);

        var handler = new DeleteHabitCommandHandler(repository.Habits, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new DeleteHabitCommand(habit.Id), TestContext.Current.CancellationToken));
        Assert.Single(repository.HabitsData);
    }

    [Fact]
    public async Task UpdateTask_ChangesFieldsForTheOwningUser()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var task = RecurringTask.Create("Original", "", TaskRepeat.Daily);
        task.AssignOwner(user.Id);
        repository.RecurringTasksData.Add(task);

        var handler = new UpdateTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(user.Id));
        await handler.Handle(
            new UpdateTaskCommand(task.Id, new SaveTaskRequest("Renamed", "Updated", TaskRepeat.Weekly)),
            TestContext.Current.CancellationToken);

        Assert.Equal("Renamed", task.Title);
        Assert.Equal(TaskRepeat.Weekly, task.Repeat);
    }

    [Fact]
    public async Task UpdateTask_RejectsAnotherUsersTask()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var task = RecurringTask.Create("Private", "", TaskRepeat.Daily);
        task.AssignOwner(other.Id);
        repository.RecurringTasksData.Add(task);

        var handler = new UpdateTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new UpdateTaskCommand(task.Id, new SaveTaskRequest("Hijacked", "", TaskRepeat.Weekly)),
            TestContext.Current.CancellationToken));
        Assert.Equal("Private", task.Title);
    }

    [Fact]
    public async Task DeleteTask_RemovesTheOwningUsersTask()
    {
        var repository = new FakeUnitOfWork();
        var user = CreateCurrentUser(repository);
        var task = RecurringTask.Create("Disposable", "", TaskRepeat.None);
        task.AssignOwner(user.Id);
        repository.RecurringTasksData.Add(task);

        var handler = new DeleteTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(user.Id));
        await handler.Handle(new DeleteTaskCommand(task.Id), TestContext.Current.CancellationToken);

        Assert.Empty(repository.RecurringTasksData);
    }

    [Fact]
    public async Task DeleteTask_RejectsAnotherUsersTask()
    {
        var repository = new FakeUnitOfWork();
        var current = CreateCurrentUser(repository);
        var other = User.Create("Other", "other@beeday.invalid");
        repository.UsersData.Add(other);
        var task = RecurringTask.Create("Private", "", TaskRepeat.None);
        task.AssignOwner(other.Id);
        repository.RecurringTasksData.Add(task);

        var handler = new DeleteTaskCommandHandler(repository.RecurringTasks, new FakeCurrentUserContext(current.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() => handler.Handle(
            new DeleteTaskCommand(task.Id), TestContext.Current.CancellationToken));
        Assert.Single(repository.RecurringTasksData);
    }

    private static User CreateCurrentUser(FakeUnitOfWork repository)
    {
        var user = User.Create("Test User", $"{Guid.NewGuid():N}@beeday.invalid");
        repository.UsersData.Add(user);
        return user;
    }
}
