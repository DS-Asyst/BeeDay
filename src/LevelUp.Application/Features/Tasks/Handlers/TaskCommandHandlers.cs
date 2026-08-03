using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Exceptions;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Features.Tasks.Handlers;

public sealed class CreateTaskCommandHandler(IRecurringTaskRepository repository, ICurrentUserContext currentUser) : IRequestHandler<CreateTaskCommand>
{
    public Task Handle(CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;
        var task = RecurringTask.Create(request.Title, request.Description, request.Repeat, request.Attribute);
        task.AssignOwner(userId);
        return repository.AddAsync(task, cancellationToken);
    }
}

public sealed class UpdateTaskCommandHandler(IRecurringTaskRepository repository, ICurrentUserContext currentUser) : IRequestHandler<UpdateTaskCommand>
{
    public async Task Handle(UpdateTaskCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await TaskLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);

        var request = command.Request;
        await repository.UpdateAsync(
            userId,
            command.Id,
            task => task.Update(request.Title, request.Description, request.Repeat, request.Attribute),
            cancellationToken);
    }
}

public sealed class ToggleTaskCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUser,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : IRequestHandler<ToggleTaskCommand>
{
    public async Task Handle(ToggleTaskCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        ExperienceEntry? experienceEntry = null;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            await TaskLookup.RequireExistsAsync(unitOfWork.RecurringTasks, userId, command.Id, cancellationToken);

            var justCompleted = false;
            string taskTitle = string.Empty;
            await unitOfWork.RecurringTasks.UpdateAsync(userId, command.Id, task =>
            {
                var wasCompleted = task.Completed;
                task.ToggleCompletion();
                justCompleted = !wasCompleted && task.Completed;
                taskTitle = task.Title;
            }, cancellationToken);

            if (justCompleted)
            {
                await unitOfWork.Users.UpdateAsync(userId, user =>
                {
                    experienceEntry = (rewards ?? new ExperienceRewardService()).Grant(
                        user,
                        ExperienceSourceType.Task,
                        command.Id,
                        ExperienceRewardType.Completion,
                        $"Task completed: {taskTitle}");
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
            await ExperienceRewardEventPublisher.PublishAsync(publisher, userId, experienceEntry, cancellationToken);
        }
    }
}

public sealed class DeleteTaskCommandHandler(IRecurringTaskRepository repository, ICurrentUserContext currentUser) : IRequestHandler<DeleteTaskCommand>
{
    public async Task Handle(DeleteTaskCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var task = await TaskLookup.RequireExistsAsync(repository, userId, command.Id, cancellationToken);
        await repository.RemoveAsync(task, cancellationToken);
    }
}

/// <summary>Preserves the exact "not found" message <c>LevelUpData.FindTask</c> used to throw.</summary>
internal static class TaskLookup
{
    public static async Task<RecurringTask> RequireExistsAsync(
        IRecurringTaskRepository repository,
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(userId, taskId, cancellationToken)
            ?? throw new InvalidDomainStateException($"Task '{taskId}' was not found for the authenticated User.");
}
