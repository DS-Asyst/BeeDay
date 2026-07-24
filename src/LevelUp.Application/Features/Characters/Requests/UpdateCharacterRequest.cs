namespace LevelUp.Application.Features.Characters.Requests;

public sealed record UpdateCharacterRequest(string Nickname, string? Avatar = null);
