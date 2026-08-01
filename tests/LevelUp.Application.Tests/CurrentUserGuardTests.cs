using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Application.Features.Habits.Handlers;
using LevelUp.Application.Features.Habits.Requests;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Application.Tests;

/// <summary>
/// Re-verifies, in the exact combined shape the Sprint 12.5 fallback-removal fix was meant to
/// cover, that <see cref="CurrentUserGuard.RequireUserId"/> never falls back to
/// <see cref="LevelUpData.CurrentUserId"/> — a persisted document-bootstrapping field, not an
/// authentication mechanism — when the authenticated context itself has no user. The scenario:
/// <c>LevelUpData.CurrentUserId</c> points at a real, existing user AND
/// <see cref="ICurrentUserContext.UserId"/> is null at the same time. Before the Sprint 12.5 fix,
/// code that read <c>data.CurrentUserId</c> directly could have treated this as "logged in as that
/// user"; the fix requires every mutation to go through the authenticated context only.
/// </summary>
public sealed class CurrentUserGuardTests
{
    [Fact]
    public void RequireUserId_WithNullContextUserId_ThrowsEvenWhenDataCurrentUserIdPointsAtARealUser()
    {
        var repository = new Repository();
        var user = repository.AddUser("Real User", "real-user@levelup.test");
        repository.Data.SetCurrentUser(user.Id);

        Assert.Equal(user.Id, repository.Data.CurrentUserId);

        var exception = Assert.Throws<InvalidDomainStateException>(() =>
            CurrentUserGuard.RequireUserId(repository.Data, new NullUserContext()));
        Assert.Equal("An authenticated User is required.", exception.Message);
    }

    [Fact]
    public async Task Handler_WithNullContextUserId_RejectsTheOperationEvenWhenDataCurrentUserIdPointsAtARealUser()
    {
        var repository = new Repository();
        var user = repository.AddUser("Real User", "real-user@levelup.test");
        repository.Data.SetCurrentUser(user.Id);

        var handler = new CreateHabitCommandHandler(repository, new NullUserContext());
        var command = new CreateHabitCommand(new SaveHabitRequest(
            "Should never be created", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(command, TestContext.Current.CancellationToken));

        Assert.Empty(repository.Data.Habits);
    }

    private sealed class NullUserContext : ICurrentUserContext
    {
        public Guid? UserId => null;
    }

    private sealed class Repository : ILevelUpRepository
    {
        public LevelUpData Data { get; } = new();

        public User AddUser(string name, string email)
        {
            var user = User.Create(name, email);
            Data.AddUser(user);
            return user;
        }

        public Task<LevelUpData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(Data);
        public Task SaveAsync(LevelUpData data, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken = default)
        {
            mutation(Data);
            return Task.CompletedTask;
        }
    }
}
