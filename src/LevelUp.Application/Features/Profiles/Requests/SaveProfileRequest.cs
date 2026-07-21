using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Profiles.Requests;

public sealed record SaveProfileRequest(
    string Name,
    string Nickname,
    CharacterClass CharacterClass);
