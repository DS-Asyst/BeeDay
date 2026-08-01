using LevelUp.Application.Features.Authentication.Requests;
using LevelUp.Application.Features.Authentication.Responses;
using MediatR;

namespace LevelUp.Application.Features.Authentication.Commands;

public sealed record AuthenticateUserCommand(AuthenticateUserRequest Request) : IRequest<AuthenticatedUserResponse>;
