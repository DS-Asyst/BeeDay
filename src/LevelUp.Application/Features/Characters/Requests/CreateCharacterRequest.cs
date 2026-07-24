using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Characters.Requests;

public sealed record CreateCharacterRequest(
    string UserName,
    string Nickname,
    CharacterClass CharacterClass,
    string? Avatar = null);
