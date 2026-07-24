using LevelUp.Application.Features.Users.Requests;
using MediatR;

namespace LevelUp.Application.Features.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<Guid>;
public sealed record UpdateCurrentUserPreferencesCommand(UpdateUserPreferencesRequest Request) : IRequest;
public sealed record UpdateCurrentUserAccountCommand(UpdateUserAccountRequest Request) : IRequest;
