using BeeDay.Application.Features.Users.Requests;
using MediatR;

namespace BeeDay.Application.Features.Users.Commands;

public sealed record CreateUserCommand(CreateUserRequest Request) : IRequest<Guid>;
public sealed record CreateAccountCommand(CreateAccountRequest Request) : IRequest<Guid>;
public sealed record CompleteUserProfileCommand(CompleteUserProfileRequest Request) : IRequest;
public sealed record UpdateCurrentUserAvatarCommand(UpdateUserAvatarRequest Request) : IRequest;
public sealed record UpdateCurrentUserPreferencesCommand(UpdateUserPreferencesRequest Request) : IRequest;
public sealed record UpdateCurrentUserAccountCommand(UpdateUserAccountRequest Request) : IRequest;
public sealed record ChangeCurrentUserPasswordCommand(ChangeUserPasswordRequest Request) : IRequest;
public sealed record CompleteCurrentUserOnboardingCommand : IRequest;
