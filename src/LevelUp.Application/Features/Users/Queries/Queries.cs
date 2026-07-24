using LevelUp.Application.Features.Users.Responses;
using MediatR;

namespace LevelUp.Application.Features.Users.Queries;

public sealed record GetCurrentUserQuery : IRequest<CurrentUserResponse?>;
public sealed record GetCurrentCharacterQuery : IRequest<CurrentCharacterResponse?>;
