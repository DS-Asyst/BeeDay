using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Habits.Commands;
using LevelUp.Domain.Entities;
using MediatR;
namespace LevelUp.Application.Features.Habits.Handlers;

public sealed class CreateHabitCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<CreateHabitCommand> { public Task Handle(CreateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddHabit(Habit.Create(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter)); }, t); }
public sealed class UpdateHabitCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<UpdateHabitCommand> { public Task Handle(UpdateHabitCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; Find(d.Habits, c.Id).Update(x.Title, x.Description, x.Direction, x.Difficulty, x.ResetCounter); }, t); }
public sealed class RegisterHabitPositiveCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<RegisterHabitPositiveCommand> { public Task Handle(RegisterHabitPositiveCommand c, CancellationToken t) => MutateAsync(d => Find(d.Habits, c.Id).RegisterPositive(), t); }
public sealed class RegisterHabitNegativeCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<RegisterHabitNegativeCommand> { public Task Handle(RegisterHabitNegativeCommand c, CancellationToken t) => MutateAsync(d => Find(d.Habits, c.Id).RegisterNegative(), t); }
public sealed class DeleteHabitCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<DeleteHabitCommand> { public Task Handle(DeleteHabitCommand c, CancellationToken t) => DeleteAsync(d => d.Habits, c.Id, t); }
