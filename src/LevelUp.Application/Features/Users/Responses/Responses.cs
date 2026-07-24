using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Users.Responses;

public sealed record CurrentUserResponse(Guid Id, string Name, string Email, UserLanguage Language, UserTheme Theme, bool IsActive);
public sealed record CurrentCharacterResponse(Guid Id, Guid UserId, string Nickname, CharacterClass Class, string Avatar);
