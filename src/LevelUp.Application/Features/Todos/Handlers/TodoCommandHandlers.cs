using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Domain.Entities;
using MediatR;

namespace LevelUp.Application.Features.Todos.Handlers;

public sealed class CreateTodoCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<CreateTodoCommand>
{
    public Task Handle(CreateTodoCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var request = command.Request;
            data.FindProject(request.ProjectId).AddTodo(Todo.Create(request.ProjectId, request.Title, request.Description, request.DueDate));
        }, cancellationToken);
}

public sealed class UpdateTodoCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<UpdateTodoCommand>
{
    public Task Handle(UpdateTodoCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var request = command.Request;
            var found = data.FindTodo(command.Id);
            if (found.Project.Id == request.ProjectId)
            {
                found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate);
                return;
            }

            found.Project.RemoveTodo(command.Id);
            found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate);
            data.FindProject(request.ProjectId).AddTodo(found.Todo);
        }, cancellationToken);
}

public sealed class ToggleTodoCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<ToggleTodoCommand>
{
    public Task Handle(ToggleTodoCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data => data.FindTodo(command.Id).Todo.ToggleCompletion(), cancellationToken);
}

public sealed class DeleteTodoCommandHandler(ILevelUpRepository repository) : RequestHandlerBase(repository), IRequestHandler<DeleteTodoCommand>
{
    public Task Handle(DeleteTodoCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var found = data.FindTodo(command.Id);
            found.Project.RemoveTodo(command.Id);
        }, cancellationToken);
}
