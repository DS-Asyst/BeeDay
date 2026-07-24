using LevelUp.Application.Features.Projects.Requests;
using MediatR;

namespace LevelUp.Application.Features.Projects.Commands;

public sealed record CreateProjectCommand(SaveProjectRequest Request) : IRequest;
public sealed record UpdateProjectCommand(Guid Id, SaveProjectRequest Request) : IRequest;
public sealed record DeleteProjectCommand(Guid Id) : IRequest;
