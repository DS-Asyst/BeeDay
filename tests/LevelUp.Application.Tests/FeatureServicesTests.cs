using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Exceptions;
using LevelUp.Application.Features.Dashboard.Services;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Application.Features.Habits.Services;
using LevelUp.Application.Features.Profiles.Requests;
using LevelUp.Application.Features.Profiles.Services;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Xunit;

namespace LevelUp.Application.Tests;

public sealed class FeatureServicesTests
{
    [Fact]
    public async Task ProfileService_CreatesProfile()
    {
        var repository = new InMemoryLevelUpRepository();
        var service = new ProfileService(repository);

        await service.SaveAsync(
            new SaveProfileRequest("Tiago", "T", CharacterClass.Warrior),
            TestContext.Current.CancellationToken);

        Assert.NotNull(repository.Data.Profile);
        Assert.Equal("Tiago", repository.Data.Profile.Name);
    }

    [Fact]
    public async Task HabitService_AddsHabit()
    {
        var repository = new InMemoryLevelUpRepository();
        var service = new HabitService(repository);

        await service.AddAsync(
            new SaveHabitRequest(
                "Study",
                "Study ASP.NET Core",
                HabitDirection.Positive,
                HabitDifficulty.Medium,
                HabitResetCounter.Daily),
            TestContext.Current.CancellationToken);

        var habit = Assert.Single(repository.Data.Habits);
        Assert.Equal("Study", habit.Title);
    }

    [Fact]
    public async Task HabitService_ThrowsWhenHabitDoesNotExist()
    {
        var repository = new InMemoryLevelUpRepository();
        var service = new HabitService(repository);

        await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => service.RegisterPositiveAsync(
                Guid.NewGuid(),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task QueryService_ReturnsRepositoryData()
    {
        var repository = new InMemoryLevelUpRepository();
        var service = new LevelUpQueryService(repository);

        var response = await service.GetAsync(TestContext.Current.CancellationToken);

        Assert.Same(repository.Data, response.Data);
    }

    private sealed class InMemoryLevelUpRepository : ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();

        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Data);

        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(
            Action<LevelUpData> mutation,
            CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
