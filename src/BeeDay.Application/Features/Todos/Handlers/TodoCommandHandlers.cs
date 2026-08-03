using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Experience;
using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Projects.Handlers;
using BeeDay.Application.Features.Todos.Commands;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Exceptions;
using BeeDay.Domain.Experience;
using MediatR;

namespace BeeDay.Application.Features.Todos.Handlers;

public sealed class CreateTodoCommandHandler(IProjectRepository repository, ICurrentUserContext currentUser) : IRequestHandler<CreateTodoCommand>
{
    public async Task Handle(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;
        await ProjectLookup.RequireExistsAsync(repository, userId, request.ProjectId, cancellationToken);

        var todo = Todo.Create(request.ProjectId, request.Title, request.Description, request.DueDate, request.Attribute);
        todo.AssignOwner(userId);
        await repository.AddTodoAsync(userId, request.ProjectId, todo, cancellationToken);
    }
}

public sealed class UpdateTodoCommandHandler(
    IProjectRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser) : IRequestHandler<UpdateTodoCommand>
{
    public async Task Handle(UpdateTodoCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;

        var currentProject = await TodoLookup.RequireOwningProjectAsync(repository, userId, command.Id, cancellationToken);
        await ProjectLookup.RequireExistsAsync(repository, userId, request.ProjectId, cancellationToken);

        if (currentProject.Id == request.ProjectId)
        {
            // Same Project — a single UpdateTodoAsync call is already atomic on its own, no transaction needed.
            await repository.UpdateTodoAsync(
                userId,
                command.Id,
                todo => todo.Update(request.ProjectId, request.Title, request.Description, request.DueDate, request.Attribute),
                cancellationToken);
            return;
        }

        // Moving to a different Project needs both the field update and the move to persist together —
        // two separate repository calls would not be atomic without an explicit transaction (the field
        // update could persist while the move fails, or vice versa). This is exactly the "Project + Todo"
        // cross-Aggregate boundary named for IUnitOfWork.
        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await unitOfWork.Projects.UpdateTodoAsync(
                userId,
                command.Id,
                todo => todo.Update(currentProject.Id, request.Title, request.Description, request.DueDate, request.Attribute),
                cancellationToken);
            await unitOfWork.Projects.MoveTodoAsync(userId, command.Id, request.ProjectId, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}

public sealed class ToggleTodoCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : IRequestHandler<ToggleTodoCommand>
{
    public async Task Handle(ToggleTodoCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        ExperienceEntry? todoExperienceEntry = null;
        ExperienceEntry? projectExperienceEntry = null;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var found = await unitOfWork.Projects.GetByTodoIdAsync(userId, command.Id, cancellationToken)
                ?? throw new InvalidDomainStateException($"To-Do '{command.Id}' was not found for the authenticated User.");
            var todo = found.Todos.Single(existing => existing.Id == command.Id);
            var projectWasCompleted = found.Completed;
            var todoWasCompleted = todo.Completed;
            var todoTitle = todo.Title;

            var todoJustCompleted = false;
            await unitOfWork.Projects.UpdateTodoAsync(userId, command.Id, tracked =>
            {
                tracked.ToggleCompletion();
                todoJustCompleted = !todoWasCompleted && tracked.Completed;
            }, cancellationToken);

            if (todoJustCompleted)
            {
                await unitOfWork.Users.UpdateAsync(userId, user =>
                {
                    todoExperienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                        user,
                        ExperienceSourceType.Todo,
                        command.Id,
                        ExperienceRewardType.Completion,
                        $"To-Do completed: {todoTitle}");
                }, cancellationToken);
            }

            var reloaded = await unitOfWork.Projects.GetAsync(userId, found.Id, cancellationToken);
            if (reloaded is not null && !projectWasCompleted && reloaded.Completed)
            {
                await unitOfWork.Users.UpdateAsync(userId, user =>
                {
                    projectExperienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                        user,
                        ExperienceSourceType.Project,
                        found.Id,
                        ExperienceRewardType.Completion,
                        $"Project completed: {found.Title}");
                }, cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        if (publisher is not null)
        {
            await ExperienceRewardEventPublisher.PublishAsync(publisher, userId, todoExperienceEntry, cancellationToken);
            await ExperienceRewardEventPublisher.PublishAsync(publisher, userId, projectExperienceEntry, cancellationToken);
        }
    }
}

public sealed class DeleteTodoCommandHandler(IProjectRepository repository, ICurrentUserContext currentUser) : IRequestHandler<DeleteTodoCommand>
{
    public async Task Handle(DeleteTodoCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await TodoLookup.RequireOwningProjectAsync(repository, userId, command.Id, cancellationToken);
        await repository.RemoveTodoAsync(userId, command.Id, cancellationToken);
    }
}

/// <summary>Preserves the exact "not found" message <c>LevelUpData.FindTodo(userId, todoId)</c> used to throw.</summary>
internal static class TodoLookup
{
    public static async Task<Project> RequireOwningProjectAsync(
        IProjectRepository repository,
        Guid userId,
        Guid todoId,
        CancellationToken cancellationToken) =>
        await repository.GetByTodoIdAsync(userId, todoId, cancellationToken)
            ?? throw new InvalidDomainStateException($"To-Do '{todoId}' was not found for the authenticated User.");
}
