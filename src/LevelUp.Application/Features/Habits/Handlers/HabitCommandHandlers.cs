using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Domain.Entities;
using MediatR;
namespace LevelUp.Application.Features.Habits.Handlers;

public sealed class CreateHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<CreateHabitCommand>
{
    public Task Handle(CreateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddHabit(CurrentUserGuard.RequireUserId(d, currentUser), Habit.Create(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter)); }, t);
}
public sealed class UpdateHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<UpdateHabitCommand>
{
    public Task Handle(UpdateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).Update(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter); }, t);
}
public sealed class RegisterHabitPositiveCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<RegisterHabitPositiveCommand>
{
    public Task Handle(RegisterHabitPositiveCommand c, CancellationToken t) => MutateAsync(d => d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).RegisterPositive(), t);
}
public sealed class RegisterHabitNegativeCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<RegisterHabitNegativeCommand>
{
    public Task Handle(RegisterHabitNegativeCommand c, CancellationToken t) => MutateAsync(d => d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).RegisterNegative(), t);
}
public sealed class DeleteHabitCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<DeleteHabitCommand>
{
    public Task Handle(DeleteHabitCommand c, CancellationToken t) => MutateAsync(d => d.Habits.Remove(d.FindHabit(CurrentUserGuard.RequireUserId(d, currentUser), c.Id)), t);
}
