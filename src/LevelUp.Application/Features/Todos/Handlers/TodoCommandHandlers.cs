using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Todos.Handlers;

public sealed class CreateTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<CreateTodoCommand>
{
    public Task Handle(CreateTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        var request = command.Request;
        var project = data.FindProject(userId, request.ProjectId);
        var todo = Todo.Create(request.ProjectId, request.Title, request.Description, request.DueDate);
        todo.AssignOwner(userId);
        project.AddTodo(todo);
    }, cancellationToken);
}

public sealed class UpdateTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<UpdateTodoCommand>
{
    public Task Handle(UpdateTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        var request = command.Request;
        var found = data.FindTodo(userId, command.Id);
        var destination = data.FindProject(userId, request.ProjectId);
        if (found.Project.Id == destination.Id)
        {
            found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate);
            return;
        }
        found.Project.RemoveTodo(command.Id);
        found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate);
        destination.AddTodo(found.Todo);
    }, cancellationToken);
}

public sealed class ToggleTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<ToggleTodoCommand>
{
    public Task Handle(ToggleTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data => data.FindTodo(CurrentUserGuard.RequireUserId(data, currentUser), command.Id).Todo.ToggleCompletion(), cancellationToken);
}

public sealed class DeleteTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<DeleteTodoCommand>
{
    public Task Handle(DeleteTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var found = data.FindTodo(CurrentUserGuard.RequireUserId(data, currentUser), command.Id);
        found.Project.RemoveTodo(command.Id);
    }, cancellationToken);
}
