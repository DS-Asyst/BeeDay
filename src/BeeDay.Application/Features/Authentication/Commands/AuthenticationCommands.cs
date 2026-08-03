using BeeDay.Application.Features.Authentication.Requests;
using BeeDay.Application.Features.Authentication.Responses;
using MediatR;

namespace BeeDay.Application.Features.Authentication.Commands;

public sealed record AuthenticateUserCommand(AuthenticateUserRequest Request) : IRequest<AuthenticatedUserResponse>;
