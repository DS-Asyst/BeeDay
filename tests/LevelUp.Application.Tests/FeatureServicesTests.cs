using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Ordering.Handlers;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Handlers;
using LevelUp.Application.Features.Users.Queries;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact]
    public async Task CompleteUserProfileHandler_SetsNicknameAndFullName()
    {
        var r = new FakeUnitOfWork();
        var user = CreateCurrentUser(r);
        await new CompleteUserProfileCommandHandler(r.Users, new FakeCurrentUserContext(user.Id))
            .Handle(new CompleteUserProfileCommand(new("Tiago", "tiago")), TestContext.Current.CancellationToken);

        Assert.Equal("Tiago", user.Name);
        Assert.True(user.HasProfile);
        Assert.Equal("tiago", user.Nickname);
    }

    [Fact]
    public async Task CompleteUserProfileHandler_RejectsNicknameAlreadyUsedByAnotherUser()
    {
        var r = new FakeUnitOfWork();
        var other = User.Create("Other", "other@levelup.invalid");
        other.CompleteProfile("tiago", null);
        r.UsersData.Add(other);
        var user = CreateCurrentUser(r);
        var handler = new CompleteUserProfileCommandHandler(r.Users, new FakeCurrentUserContext(user.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(new CompleteUserProfileCommand(new("Tiago", "tiago")), TestContext.Current.CancellationToken));
    }
    [Fact] public async Task CreateHabitHandler_AddsHabit() { var r = new FakeUnitOfWork(); var user = CreateCurrentUser(r); await new CreateHabitCommandHandler(r.Habits, new FakeCurrentUserContext(user.Id)).Handle(new(new("Study", "Study ASP.NET Core", HabitDirection.Positive, HabitDifficulty.Medium, HabitResetCounter.Daily)), TestContext.Current.CancellationToken); Assert.Equal("Study", Assert.Single(r.HabitsData).Title); }
    [Fact]
    public async Task RegisterHabitPositiveHandler_ThrowsWhenMissing()
    {
        var r = new FakeUnitOfWork();
        var user = CreateCurrentUser(r);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            new RegisterHabitPositiveCommandHandler(r, new FakeCurrentUserContext(user.Id))
                .Handle(new(Guid.NewGuid()), TestContext.Current.CancellationToken));
    }
    [Fact] public async Task ReorderHandler_ReordersTasks() { var r = new FakeUnitOfWork(); var user = CreateCurrentUser(r); var a = RecurringTask.Create("First", "", TaskRepeat.None); var b = RecurringTask.Create("Second", "", TaskRepeat.None); a.AssignOwner(user.Id); b.AssignOwner(user.Id); r.RecurringTasksData.Add(a); r.RecurringTasksData.Add(b); await new ReorderActivitiesCommandHandler(r.Habits, r.RecurringTasks, r.Projects, new FakeCurrentUserContext(user.Id)).Handle(new(new(ActivityCollection.Tasks, [b.Id, a.Id])), TestContext.Current.CancellationToken); Assert.Equal([b.Id, a.Id], r.RecurringTasksData.Select(x => x.Id)); }

    /// <summary>
    /// Locks in behavior re-verified during Sprint 14.7's audit of <c>LevelUpData</c> (removed that
    /// Sprint): the userId-scoped reorder path production actually uses already rejected any id not
    /// owned by the caller — including a ghost id that belongs to nobody — with
    /// <see cref="InvalidDomainStateException"/> (409), before Sprint 14.6 existed. The Domain test that
    /// used to assert an <see cref="ArgumentException"/> (<c>ActivityOrderingTests.ReorderRejectsUnknownIdentifier</c>,
    /// removed with <c>LevelUpData</c>) called the single-argument, ambient-"current user" overload that
    /// no production handler ever used — not this path. No behavior changed; this test only makes the
    /// already-correct behavior explicit at the Aggregate boundary the handler actually exercises.
    /// </summary>
    [Fact]
    public async Task ReorderHandler_RejectsGenuinelyUnknownIdentifier()
    {
        var r = new FakeUnitOfWork();
        var user = CreateCurrentUser(r);
        var task = RecurringTask.Create("Only task", "", TaskRepeat.None);
        task.AssignOwner(user.Id);
        r.RecurringTasksData.Add(task);
        var handler = new ReorderActivitiesCommandHandler(r.Habits, r.RecurringTasks, r.Projects, new FakeCurrentUserContext(user.Id));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(new(new(ActivityCollection.Tasks, [task.Id, Guid.NewGuid()])), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryHandler_ReturnsAuthenticatedUserSnapshot()
    {
        var r = new FakeUnitOfWork();
        var user = CreateCurrentUser(r);
        var response = await new GetCurrentUserQueryHandler(r.Users, new FakeCurrentUserContext(user.Id))
            .Handle(new GetCurrentUserQuery(), TestContext.Current.CancellationToken);

        Assert.NotNull(response);
        Assert.Equal(user.Id, response.Id);
    }

    private static User CreateCurrentUser(FakeUnitOfWork repository)
    {
        var user = User.Create("Test User", "test-user@levelup.invalid");
        repository.UsersData.Add(user);
        return user;
    }
}
