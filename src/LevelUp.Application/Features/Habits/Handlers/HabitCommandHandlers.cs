using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Features.Habits.Handlers;

public sealed class CreateHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext currentUser) : RequestHandlerBase(r), IRequestHandler<CreateHabitCommand>
{
    public Task Handle(CreateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddHabit(CurrentUserGuard.RequireUserId(d, currentUser), Habit.Create(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter, x.Attribute)); }, t);
}

public sealed class UpdateHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext currentUser) : RequestHandlerBase(r), IRequestHandler<UpdateHabitCommand>
{
    public Task Handle(UpdateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).Update(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter, x.Attribute); }, t);
}

public sealed class RegisterHabitPositiveCommandHandler(
    ILevelUpRepository repository,
    ICurrentUserContext currentUser,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : RequestHandlerBase(repository), IRequestHandler<RegisterHabitPositiveCommand>
{
    public async Task Handle(RegisterHabitPositiveCommand command, CancellationToken cancellationToken)
    {
        ExperienceEntry? experienceEntry = null;
        Guid userId = Guid.Empty;

        await MutateAsync(data =>
        {
            userId = CurrentUserGuard.RequireUserId(data, currentUser);
            var habit = data.FindHabit(userId, command.Id);
            var previousCount = habit.PositiveCount;
            habit.RegisterPositive();

            if (habit.PositiveCount == previousCount)
            {
                return;
            }

            experienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                data,
                userId,
                ExperienceSourceType.Habit,
                Guid.NewGuid(),
                ExperienceRewardType.Completion,
                $"Positive habit registered: {habit.Title}");
        }, cancellationToken);

        if (publisher is not null)
        {
            await ExperienceRewardEventPublisher.PublishAsync(
                publisher,
                userId,
                experienceEntry,
                cancellationToken);
        }
    }
}

public sealed class RegisterHabitNegativeCommandHandler(ILevelUpRepository r, ICurrentUserContext currentUser) : RequestHandlerBase(r), IRequestHandler<RegisterHabitNegativeCommand>
{
    public Task Handle(RegisterHabitNegativeCommand c, CancellationToken t) => MutateAsync(d => d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).RegisterNegative(), t);
}

public sealed class DeleteHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext currentUser) : RequestHandlerBase(r), IRequestHandler<DeleteHabitCommand>
{
    public Task Handle(DeleteHabitCommand c, CancellationToken t) => MutateAsync(d => d.Habits.Remove(d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id)), t);
}
