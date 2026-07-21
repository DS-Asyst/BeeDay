using LevelUp.Application.Common.Caching;
using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Exceptions;
using LevelUp.Application.Features.Dashboard.Handlers;
using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Ordering.Handlers;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Application.Features.Profiles.Handlers;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Xunit;
namespace LevelUp.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact] public async Task SaveProfileHandler_CreatesProfile() { var r = new Repo(); await new SaveProfileCommandHandler(r).Handle(new(new("Tiago", "tiago", CharacterClass.Warrior)), TestContext.Current.CancellationToken); Assert.Equal("Tiago", r.Data.Profile!.Name); }
    [Fact] public async Task CreateHabitHandler_AddsHabit() { var r = new Repo(); await new CreateHabitCommandHandler(r).Handle(new(new("Study", "Study ASP.NET Core", HabitDirection.Positive, HabitDifficulty.Medium, HabitResetCounter.Daily)), TestContext.Current.CancellationToken); Assert.Equal("Study", Assert.Single(r.Data.Habits).Title); }
    [Fact] public async Task RegisterHabitPositiveHandler_ThrowsWhenMissing() { var r = new Repo(); await Assert.ThrowsAsync<ActivityNotFoundException>(() => new RegisterHabitPositiveCommandHandler(r).Handle(new(Guid.NewGuid()), TestContext.Current.CancellationToken)); }
    [Fact] public async Task ReorderHandler_ReordersTasks() { var r = new Repo(); var a = RecurringTask.Create("First", "", TaskRepeat.None); var b = RecurringTask.Create("Second", "", TaskRepeat.None); r.Data.AddTask(a); r.Data.AddTask(b); await new ReorderActivitiesCommandHandler(r).Handle(new(new(ActivityCollection.Tasks, [b.Id, a.Id])), TestContext.Current.CancellationToken); Assert.Equal([b.Id, a.Id], r.Data.Tasks.Select(x => x.Id)); }
    [Fact] public async Task QueryHandler_ReturnsData() { var r = new Repo(); var x = await new GetLevelUpQueryHandler(r, new Cache()).Handle(new(), TestContext.Current.CancellationToken); Assert.Same(r.Data, x.Data); }
    private sealed class Cache : IApplicationCache { public Task<T> GetOrCreateAsync<T>(string k, Func<CancellationToken, Task<T>> f, TimeSpan d, CancellationToken c) => f(c); public void Remove(string k) { } }
    private sealed class Repo : ILevelUpRepository { public LevelUpData Data { get; } = new(); public Task<LevelUpData> LoadAsync(CancellationToken c = default) => Task.FromResult(Data); public Task SaveAsync(LevelUpData d, CancellationToken c = default) => Task.CompletedTask; public Task UpdateAsync(Action<LevelUpData> m, CancellationToken c = default) { m(Data); return Task.CompletedTask; } }
}
