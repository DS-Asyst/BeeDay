using BeeDay.Application.Features.Todos.Requests;
using MediatR;

namespace BeeDay.Application.Features.Todos.Commands;

public sealed record CreateTodoCommand(SaveTodoRequest Request) : IRequest;
public sealed record UpdateTodoCommand(Guid Id, SaveTodoRequest Request) : IRequest;
public sealed record ToggleTodoCommand(Guid Id) : IRequest;
public sealed record DeleteTodoCommand(Guid Id) : IRequest;
