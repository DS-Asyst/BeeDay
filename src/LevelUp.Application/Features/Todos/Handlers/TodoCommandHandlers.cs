using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Todos.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Features.Todos.Handlers;

public sealed class CreateTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<CreateTodoCommand>
{
    public Task Handle(CreateTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var userId = CurrentUserGuard.RequireUserId(data, currentUser);
        var request = command.Request;
        var project = data.FindProject(userId, request.ProjectId);
        var todo = Todo.Create(request.ProjectId, request.Title, request.Description, request.DueDate, request.Attribute);
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
            found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate, request.Attribute);
            return;
        }
        found.Project.RemoveTodo(command.Id);
        found.Todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate, request.Attribute);
        destination.AddTodo(found.Todo);
    }, cancellationToken);
}

public sealed class ToggleTodoCommandHandler(
    ILevelUpRepository repository,
    ICurrentUserContext? currentUser = null,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : RequestHandlerBase(repository), IRequestHandler<ToggleTodoCommand>
{
    public async Task Handle(ToggleTodoCommand command, CancellationToken cancellationToken)
    {
        ExperienceEntry? todoExperienceEntry = null;
        ExperienceEntry? projectExperienceEntry = null;
        Character? character = null;
        Guid userId = Guid.Empty;

        await MutateAsync(data =>
        {
            userId = CurrentUserGuard.RequireUserId(data, currentUser);
            var found = data.FindTodo(userId, command.Id);
            var projectWasCompleted = found.Project.Completed;
            var todoWasCompleted = found.Todo.Completed;

            found.Todo.ToggleCompletion();

            if (!todoWasCompleted && found.Todo.Completed)
            {
                character = data.FindCharacterForUser(userId);
                todoExperienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                    data,
                    userId,
                    ExperienceSourceType.Todo,
                    found.Todo.Id,
                    ExperienceRewardType.Completion,
                    $"To-Do completed: {found.Todo.Title}");
            }

            if (!projectWasCompleted && found.Project.Completed)
            {
                character ??= data.FindCharacterForUser(userId);
                projectExperienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                    data,
                    userId,
                    ExperienceSourceType.Project,
                    found.Project.Id,
                    ExperienceRewardType.Completion,
                    $"Project completed: {found.Project.Title}");
            }
        }, cancellationToken);

        if (character is null)
        {
            return;
        }

        if (publisher is not null)
        {
            await ExperienceRewardEventPublisher.PublishAsync(
                publisher,
                userId,
                character,
                todoExperienceEntry,
                cancellationToken);
            await ExperienceRewardEventPublisher.PublishAsync(
                publisher,
                userId,
                character,
                projectExperienceEntry,
                cancellationToken);
        }
    }
}

public sealed class DeleteTodoCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null) : RequestHandlerBase(repository), IRequestHandler<DeleteTodoCommand>
{
    public Task Handle(DeleteTodoCommand command, CancellationToken cancellationToken) => MutateAsync(data =>
    {
        var found = data.FindTodo(CurrentUserGuard.RequireUserId(data, currentUser), command.Id);
        found.Project.RemoveTodo(command.Id);
    }, cancellationToken);
}
