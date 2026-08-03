using BeeDay.Application.Features.Projects.Requests;
using MediatR;

namespace BeeDay.Application.Features.Projects.Commands;

public sealed record CreateProjectCommand(SaveProjectRequest Request) : IRequest;
public sealed record UpdateProjectCommand(Guid Id, SaveProjectRequest Request) : IRequest;
public sealed record DeleteProjectCommand(Guid Id) : IRequest;
