using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Characters.Requests;

public sealed record CreateAccountRequest(
    string Name,
    string Email,
    string Password,
    string Nickname,
    CharacterClass CharacterClass,
    string? Avatar = null);
