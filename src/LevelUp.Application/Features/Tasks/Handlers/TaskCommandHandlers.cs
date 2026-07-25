using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Domain.Entities;
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
public sealed class ToggleTaskCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<ToggleTaskCommand>
{
    public Task Handle(ToggleTaskCommand c, CancellationToken t) => MutateAsync(d => d.FindTask(CurrentUserGuard.RequireUserId(d, currentUser), c.Id).ToggleCompletion(), t);
}
public sealed class DeleteTaskCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<DeleteTaskCommand>
{
    public Task Handle(DeleteTaskCommand c, CancellationToken t) => MutateAsync(d => d.Tasks.Remove(d.FindTask(CurrentUserGuard.RequireUserId(d, currentUser), c.Id)), t);
}
