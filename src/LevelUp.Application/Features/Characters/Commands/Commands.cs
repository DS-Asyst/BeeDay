using LevelUp.Application.Features.Characters.Requests;
using MediatR;

namespace LevelUp.Application.Features.Characters.Commands;

public sealed record CreateCharacterCommand(CreateCharacterRequest Request) : IRequest;
public sealed record UpdateCurrentCharacterCommand(UpdateCharacterRequest Request) : IRequest;
