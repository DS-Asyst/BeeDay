using LevelUp.Application.Common.Caching;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Features.Characters.Commands;
using LevelUp.Application.Features.Characters.Handlers;
using LevelUp.Application.Features.Dashboard.Handlers;
using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Ordering.Handlers;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact]
    public async Task CreateCharacterHandler_SeparatesUserAndCharacter()
    {
        var r = new Repo();
        var user = r.CreateCurrentUser();
        await new CreateCharacterCommandHandler(r, new UserContext(user.Id))
            .Handle(new CreateCharacterCommand(new("Tiago", "tiago", CharacterClass.Warrior)), TestContext.Current.CancellationToken);

        Assert.Equal("Tiago", r.Data.CurrentUser!.Name);
        Assert.Equal("tiago", r.Data.CurrentCharacter!.Nickname);
        Assert.Equal(r.Data.CurrentUser.Id, r.Data.CurrentCharacter.UserId);
    }
    [Fact] public async Task CreateHabitHandler_AddsHabit() { var r = new Repo(); r.CreateCurrentUser(); await new CreateHabitCommandHandler(r).Handle(new(new("Study", "Study ASP.NET Core", HabitDirection.Positive, HabitDifficulty.Medium, HabitResetCounter.Daily)), TestContext.Current.CancellationToken); Assert.Equal("Study", Assert.Single(r.Data.Habits).Title); }
    [Fact]
    public async Task RegisterHabitPositiveHandler_ThrowsWhenMissing()
    {
        var r = new Repo();
        var user = r.CreateCurrentUser();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            new RegisterHabitPositiveCommandHandler(r, new UserContext(user.Id))
                .Handle(new(Guid.NewGuid()), TestContext.Current.CancellationToken));
    }
    [Fact] public async Task ReorderHandler_ReordersTasks() { var r = new Repo(); r.CreateCurrentUser(); var a = RecurringTask.Create("First", "", TaskRepeat.None); var b = RecurringTask.Create("Second", "", TaskRepeat.None); r.Data.AddTask(a); r.Data.AddTask(b); await new ReorderActivitiesCommandHandler(r).Handle(new(new(ActivityCollection.Tasks, [b.Id, a.Id])), TestContext.Current.CancellationToken); Assert.Equal([b.Id, a.Id], r.Data.Tasks.Select(x => x.Id)); }
    [Fact]
    public async Task QueryHandler_ReturnsAuthenticatedUserSnapshot()
    {
        var r = new Repo();
        var user = r.CreateCurrentUser();
        var x = await new GetLevelUpQueryHandler(r, new Cache(), new UserContext(user.Id))
            .Handle(new(), TestContext.Current.CancellationToken);

        Assert.Equal(user.Id, x.Data.CurrentUserId);
        Assert.Equal(user.Id, x.Data.CurrentUser!.Id);
    }
    private sealed record UserContext(Guid Id) : LevelUp.Application.Common.Security.ICurrentUserContext
    {
        public Guid? UserId => Id;
    }

    private sealed class Cache : IApplicationCache { public Task<T> GetOrCreateAsync<T>(string k, Func<CancellationToken, Task<T>> f, TimeSpan d, CancellationToken c) => f(c); public void Remove(string k) { } }
    private sealed class Repo : ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();
        public User CreateCurrentUser()
        {
            var user = User.Create("Test User", "test-user@levelup.invalid");
            Data.AddUser(user);
            Data.SetCurrentUser(user.Id);
            return user;
        }
        public Task<LevelUpData> LoadAsync(CancellationToken c = default) => Task.FromResult(Data);
        public Task SaveAsync(LevelUpData d, CancellationToken c = default) => Task.CompletedTask;
        public Task UpdateAsync(Action<LevelUpData> m, CancellationToken c = default) { m(Data); return Task.CompletedTask; }
    }
}
