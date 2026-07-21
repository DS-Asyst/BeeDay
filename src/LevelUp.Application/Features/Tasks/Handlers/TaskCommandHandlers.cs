using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Tasks.Commands;
using LevelUp.Domain.Entities;
using MediatR;
namespace LevelUp.Application.Features.Tasks.Handlers;

public sealed class CreateTaskCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<CreateTaskCommand> { public Task Handle(CreateTaskCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddTask(RecurringTask.Create(x.Title, x.Description, x.Repeat)); }, t); }
public sealed class UpdateTaskCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<UpdateTaskCommand> { public Task Handle(UpdateTaskCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; Find(d.Tasks, c.Id).Update(x.Title, x.Description, x.Repeat); }, t); }
public sealed class ToggleTaskCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<ToggleTaskCommand> { public Task Handle(ToggleTaskCommand c, CancellationToken t) => MutateAsync(d => Find(d.Tasks, c.Id).ToggleCompletion(), t); }
public sealed class DeleteTaskCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<DeleteTaskCommand> { public Task Handle(DeleteTaskCommand c, CancellationToken t) => DeleteAsync(d => d.Tasks, c.Id, t); }
