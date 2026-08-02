using LevelUp.Application.Features.Dashboard.Handlers;
using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Ordering.Handlers;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Application.Features.Users.Commands;
using LevelUp.Application.Features.Users.Handlers;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact]
    public async Task CompleteUserProfileHandler_SetsNicknameAndFullName()
    {
        var r = new FakeLevelUpRepository();
        var user = CreateCurrentUser(r);
        await new CompleteUserProfileCommandHandler(r, new FakeCurrentUserContext(user.Id))
            .Handle(new CompleteUserProfileCommand(new("Tiago", "tiago")), TestContext.Current.CancellationToken);

        Assert.Equal("Tiago", r.Data.CurrentUser!.Name);
        Assert.True(r.Data.CurrentUser.HasProfile);
        Assert.Equal("tiago", r.Data.CurrentUser.Nickname);
    }
    [Fact] public async Task CreateHabitHandler_AddsHabit() { var r = new FakeLevelUpRepository(); var user = CreateCurrentUser(r); await new CreateHabitCommandHandler(r, new FakeCurrentUserContext(user.Id)).Handle(new(new("Study", "Study ASP.NET Core", HabitDirection.Positive, HabitDifficulty.Medium, HabitResetCounter.Daily)), TestContext.Current.CancellationToken); Assert.Equal("Study", Assert.Single(r.Data.Habits).Title); }
    [Fact]
    public async Task RegisterHabitPositiveHandler_ThrowsWhenMissing()
    {
        var r = new FakeLevelUpRepository();
        var user = CreateCurrentUser(r);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            new RegisterHabitPositiveCommandHandler(r, new FakeCurrentUserContext(user.Id))
                .Handle(new(Guid.NewGuid()), TestContext.Current.CancellationToken));
    }
    [Fact] public async Task ReorderHandler_ReordersTasks() { var r = new FakeLevelUpRepository(); var user = CreateCurrentUser(r); var a = RecurringTask.Create("First", "", TaskRepeat.None); var b = RecurringTask.Create("Second", "", TaskRepeat.None); r.Data.AddTask(a); r.Data.AddTask(b); await new ReorderActivitiesCommandHandler(r, new FakeCurrentUserContext(user.Id)).Handle(new(new(ActivityCollection.Tasks, [b.Id, a.Id])), TestContext.Current.CancellationToken); Assert.Equal([b.Id, a.Id], r.Data.Tasks.Select(x => x.Id)); }
    [Fact]
    public async Task QueryHandler_ReturnsAuthenticatedUserSnapshot()
    {
        var r = new FakeLevelUpRepository();
        var user = CreateCurrentUser(r);
        var x = await new GetLevelUpQueryHandler(r, new FakeApplicationCache(), new FakeCurrentUserContext(user.Id))
            .Handle(new(), TestContext.Current.CancellationToken);

        Assert.Equal(user.Id, x.Data.CurrentUserId);
        Assert.Equal(user.Id, x.Data.CurrentUser!.Id);
    }

    private static User CreateCurrentUser(FakeLevelUpRepository repository)
    {
        var user = User.Create("Test User", "test-user@levelup.invalid");
        repository.Data.AddUser(user);
        repository.Data.SetCurrentUser(user.Id);
        return user;
    }
}
