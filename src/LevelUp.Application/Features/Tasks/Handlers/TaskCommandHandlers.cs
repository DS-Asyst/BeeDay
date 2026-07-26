using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Experience;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using LevelUp.Domain.Experience;
using MediatR;

namespace LevelUp.Application.Features.Tasks.Handlers;

public sealed class CreateTaskCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<CreateTaskCommand>
{
    public Task Handle(CreateTaskCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddTask(CurrentUserGuard.RequireUserId(d, currentUser), RecurringTask.Create(x.Title, x.Description, x.Repeat)); }, t);
}

public sealed class UpdateTaskCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<UpdateTaskCommand>
{
    public Task Handle(UpdateTaskCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.FindTask(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).Update(x.Title, x.Description, x.Repeat); }, t);
}

public sealed class ToggleTaskCommandHandler(
    ILevelUpRepository repository,
    ICurrentUserContext? currentUser = null,
    IExperienceRewardService? rewards = null,
    IPublisher? publisher = null) : RequestHandlerBase(repository), IRequestHandler<ToggleTaskCommand>
{
    public async Task Handle(ToggleTaskCommand command, CancellationToken cancellationToken)
    {
        ExperienceEntry? experienceEntry = null;
        Character? character = null;
        Guid userId = Guid.Empty;

        await MutateAsync(data =>
        {
            userId = CurrentUserGuard.RequireUserId(data, currentUser);
            var task = data.FindTask(userId, command.Id);
            var wasCompleted = task.Completed;
            task.ToggleCompletion();

            if (wasCompleted || !task.Completed)
            {
                return;
            }

            character = data.FindCharacterForUser(userId);
            experienceEntry = (rewards ?? new ExperienceRewardService()).Grant(data, userId, ExperienceSourceType.Task, task.Id, ExperienceRewardType.Completion, $"Task completed: {task.Title}");
        }, cancellationToken);

        if (character is not null && publisher is not null)
        {
            await ExperienceRewardEventPublisher.PublishAsync(
                publisher,
                userId,
                character,
                experienceEntry,
                cancellationToken);
        }
    }
}

public sealed class DeleteTaskCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<DeleteTaskCommand>
{
    public Task Handle(DeleteTaskCommand c, CancellationToken t) => MutateAsync(d => d.Tasks.Remove(d.FindTask(CurrentUserGuard.RequireUserId(d, currentUser), c.Id)), t);
}
