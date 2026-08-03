using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Habits.Commands;
using BeeDay.Application.Features.Habits.Handlers;
using BeeDay.Application.Features.Habits.Requests;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;

namespace BeeDay.Application.Tests;

/// <summary>
/// Re-verifies, in the exact combined shape the Sprint 12.5 fallback-removal fix was meant to cover,
/// that authentication never falls back to any persisted "current user" pointer — <c>ICurrentUserContext</c>
/// is the only source of the authenticated identity. <see cref="CurrentUserGuard.RequireUserId(ICurrentUserContext)"/>
/// throws purely from the authenticated context, even when a real, existing user is present elsewhere
/// in the store — there is no ambient "current user" concept left anywhere in the codebase to fall back
/// to since Sprint 14.7 removed <c>LevelUpData</c> (the last type that ever exposed one).
/// </summary>
public sealed class CurrentUserGuardTests
{
    [Fact]
    public async Task Handler_WithNullContextUserId_RejectsTheOperationEvenWhenAnotherUserExists()
    {
        var repository = new FakeUnitOfWork();
        AddUser(repository, "Real User", "real-user@beeday.test");

        var handler = new CreateHabitCommandHandler(repository.Habits, new FakeCurrentUserContext(null));
        var command = new CreateHabitCommand(new SaveHabitRequest(
            "Should never be created", "", HabitDirection.Positive, HabitDifficulty.Easy, HabitResetCounter.Daily));

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            handler.Handle(command, TestContext.Current.CancellationToken));

        Assert.Empty(repository.HabitsData);
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

    private static User AddUser(FakeUnitOfWork repository, string name, string email)
    {
        var user = User.Create(name, email);
        repository.UsersData.Add(user);
        return user;
    }
}
