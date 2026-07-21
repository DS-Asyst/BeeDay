using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Domain.Entities;
using MediatR;
namespace LevelUp.Application.Features.Todos.Handlers;

public sealed class CreateTodoCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<CreateTodoCommand> { public Task Handle(CreateTodoCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddTodo(Todo.Create(x.Title, x.Description, x.DueDate)); }, t); }
public sealed class UpdateTodoCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<UpdateTodoCommand> { public Task Handle(UpdateTodoCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; Find(d.Todos, c.Id).Update(x.Title, x.Description, x.DueDate); }, t); }
public sealed class ToggleTodoCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<ToggleTodoCommand> { public Task Handle(ToggleTodoCommand c, CancellationToken t) => MutateAsync(d => Find(d.Todos, c.Id).ToggleCompletion(), t); }
public sealed class DeleteTodoCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<DeleteTodoCommand> { public Task Handle(DeleteTodoCommand c, CancellationToken t) => DeleteAsync(d => d.Todos, c.Id, t); }
