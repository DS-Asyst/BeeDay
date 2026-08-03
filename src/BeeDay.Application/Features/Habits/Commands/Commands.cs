using BeeDay.Application.Features.Habits.Requests;
using MediatR;

namespace BeeDay.Application.Features.Habits.Commands;

public sealed record CreateHabitCommand(SaveHabitRequest Request) : IRequest;
public sealed record UpdateHabitCommand(Guid Id, SaveHabitRequest Request) : IRequest;
public sealed record RegisterHabitPositiveCommand(Guid Id) : IRequest;
public sealed record RegisterHabitNegativeCommand(Guid Id) : IRequest;
public sealed record DeleteHabitCommand(Guid Id) : IRequest;
