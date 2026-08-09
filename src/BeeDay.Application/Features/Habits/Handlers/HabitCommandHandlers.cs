using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Experience;
using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Habits.Commands;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;
using MediatR;

namespace BeeDay.Application.Features.Habits.Handlers;

public sealed class CreateHabitCommandHandler(IHabitRepository repository, ICurrentUserContext currentUser) : IRequestHandler<CreateHabitCommand>
{
    public Task Handle(CreateHabitCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;
        var habit = Habit.Create(request.Title, request.Description, request.Direction, request.Difficulty, request.ResetCounter, request.Attribute);
        habit.AssignOwner(userId);
        return repository.AddAsync(habit, cancellationToken);
    }
}

public sealed class UpdateHabitCommandHandler(IHabitRepository repository, ICurrentUserContext currentUser) : IRequestHandler<UpdateHabitCommand>
{
    public async Task Handle(UpdateHabitCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await HabitLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);

        var request = command.Request;
        await repository.UpdateAsync(
            userId,
            command.Id,
            habit => habit.Update(request.Title, request.Description, request.Direction, request.Difficulty, request.ResetCounter, request.Attribute),
            cancellationToken);
    }
}

public sealed class RegisterHabitPositiveCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : IRequestHandler<RegisterHabitPositiveCommand>
{
    public async Task Handle(RegisterHabitPositiveCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        ExperienceEntry? experienceEntry = null;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await HabitLookup.RequireExistsAsync(unitOfWork.Habits, userId, command.Id, cancellationToken);

            var countChanged = false;
            string habitTitle = string.Empty;
            await unitOfWork.Habits.UpdateAsync(userId, command.Id, habit =>
            {
                var previousCount = habit.PositiveCount;
                habit.RegisterPositive();
                countChanged = habit.PositiveCount != previousCount;
                habitTitle = habit.Title;
            }, cancellationToken);

            if (countChanged)
            {
                await unitOfWork.Users.UpdateAsync(userId, user =>
                {
                    experienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                        user,
                        ExperienceSourceType.Habit,
                        Guid.NewGuid(),
                        ExperienceRewardType.Completion,
                        $"Positive habit registered: {habitTitle}");
                }, cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        if (publisher is not null)
        {
            await ExperienceRewardEventPublisher.PublishAsync(publisher, userId, experienceEntry, cancellationToken);
        }
    }
}

public sealed class RegisterHabitNegativeCommandHandler(IHabitRepository repository, ICurrentUserContext currentUser) : IRequestHandler<RegisterHabitNegativeCommand>
{
    public async Task Handle(RegisterHabitNegativeCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await HabitLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);
        await repository.UpdateAsync(userId, command.Id, habit => habit.RegisterNegative(), cancellationToken);
    }
}

public sealed class DeleteHabitCommandHandler(IHabitRepository repository, ICurrentUserContext currentUser) : IRequestHandler<DeleteHabitCommand>
{
    public async Task Handle(DeleteHabitCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var habit = await HabitLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);
        await repository.RemoveAsync(habit, cancellationToken);
    }
}

/// <summary>
/// Preserves the exact "not found" message <c>LevelUpData.FindHabit</c> used to throw
/// (<see cref="InvalidDomainStateException"/> — the old activity-not-found-specific exception
/// mapping was removed together with the legacy <c>RequestHandlerBase.Find</c> helper it was only
/// ever wired to, confirmed by source search, never to any Habit handler) — repository methods
/// throw a generic EF/LINQ exception on a missing row, which would otherwise change the HTTP
/// status this maps to via <c>GlobalExceptionHandler</c>.
/// </summary>
internal static class HabitLookup
{
    public static async Task<Habit> RequireExistsAsync(
        IHabitRepository repository,
        Guid userId,
        Guid habitId,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(userId, habitId, cancellationToken)
            ?? throw new InvalidDomainStateException($"Habit '{habitId}' was not found for the authenticated User.");
}
