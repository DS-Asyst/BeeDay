using BeeDay.Application.Features.Users.Responses;
using MediatR;

namespace BeeDay.Application.Features.Users.Queries;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserResponse?>;
