using LevelUp.Application.Features.Tasks.Requests;
using MediatR;

namespace LevelUp.Application.Features.Tasks.Commands;

public sealed record CreateTaskCommand(SaveTaskRequest Request) : IRequest;
public sealed record UpdateTaskCommand(Guid Id, SaveTaskRequest Request) : IRequest;
public sealed record ToggleTaskCommand(Guid Id) : IRequest;
public sealed record DeleteTaskCommand(Guid Id) : IRequest;
