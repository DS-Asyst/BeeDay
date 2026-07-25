using LevelUp.Application.Features.Characters.Requests;
using MediatR;

namespace LevelUp.Application.Features.Characters.Commands;

public sealed record CreateAccountCommand(CreateAccountRequest Request) : IRequest<Guid>;
public sealed record CreateCharacterCommand(CreateCharacterRequest Request) : IRequest;
public sealed record UpdateCurrentCharacterAvatarCommand(UpdateCharacterAvatarRequest Request) : IRequest;
