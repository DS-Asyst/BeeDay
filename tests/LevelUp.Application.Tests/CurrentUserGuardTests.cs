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
/// cover, that <see cref="CurrentUserGuard.RequireUserId(LevelUpData, ICurrentUserContext)"/> never falls back to
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
        var repository = new FakeLevelUpRepository();
        var user = AddUser(repository, "Real User", "real-user@levelup.test");
        repository.Data.SetCurrentUser(user.Id);

        Assert.Equal(user.Id, repository.Data.CurrentUserId);

        var exception = Assert.Throws<InvalidDomainStateException>(() =>
            CurrentUserGuard.RequireUserId(repository.Data, new FakeCurrentUserContext(null)));
        Assert.Equal("An authenticated User is required.", exception.Message);
    }

    [Fact]
    public async Task Handler_WithNullContextUserId_RejectsTheOperationEvenWhenDataCurrentUserIdPointsAtARealUser()
    {
        var repository = new FakeLevelUpRepository();
        var user = AddUser(repository, "Real User", "real-user@levelup.test");
        repository.Data.SetCurrentUser(user.Id);

        var handler = new CreateHabitCommandHandler(repository, new FakeCurrentUserContext(null));
        var command = new CreateHabitCommand(new SaveHabitRequest(
            "Should never be created", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(command, TestContext.Current.CancellationToken));

        Assert.Empty(repository.Data.Habits);
    }

    /// <summary>
    /// Covers the single-argument overload introduced in Sprint 13.4 for handlers migrated off
    /// <c>LevelUpData</c> (docs/architecture/07-persistence-contracts.md). It only extracts the
    /// claim — existence/ownership is deliberately not its job anymore; the next Aggregate repository
    /// call in the handler is responsible for that (see the overload's XML doc).
    /// </summary>
    [Fact]
    public void RequireUserId_SingleArgumentOverload_WithNullContextUserId_Throws()
    {
        var exception = Assert.Throws<InvalidDomainStateException>(() =>
            CurrentUserGuard.RequireUserId(new FakeCurrentUserContext(null)));
        Assert.Equal("An authenticated User is required.", exception.Message);
    }

    [Fact]
    public void RequireUserId_SingleArgumentOverload_ReturnsTheClaimedId_WithoutTouchingAnyRepository()
    {
        var claimedId = Guid.NewGuid();

        var result = CurrentUserGuard.RequireUserId(new FakeCurrentUserContext(claimedId));

        Assert.Equal(claimedId, result);
    }

    private static User AddUser(FakeLevelUpRepository repository, string name, string email)
    {
        var user = User.Create(name, email);
        repository.Data.AddUser(user);
        return user;
    }
}
