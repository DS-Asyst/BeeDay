using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Projects.Commands;
using LevelUp.Domain.Entities;
using MediatR;
namespace LevelUp.Application.Features.Projects.Handlers;

public sealed class CreateProjectCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<CreateProjectCommand> { public Task Handle(CreateProjectCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; d.AddProject(Project.Create(x.Title, x.Description, x.Status)); }, t); }
public sealed class UpdateProjectCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<UpdateProjectCommand> { public Task Handle(UpdateProjectCommand c, CancellationToken t) => MutateAsync(d => { var x = c.Request; Find(d.Projects, c.Id).Update(x.Title, x.Description, x.Status); }, t); }
public sealed class ToggleProjectCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<ToggleProjectCommand> { public Task Handle(ToggleProjectCommand c, CancellationToken t) => MutateAsync(d => Find(d.Projects, c.Id).ToggleStatus(), t); }
public sealed class DeleteProjectCommandHandler(ILevelUpRepository r) : RequestHandlerBase(r), IRequestHandler<DeleteProjectCommand> { public Task Handle(DeleteProjectCommand c, CancellationToken t) => DeleteAsync(d => d.Projects, c.Id, t); }
