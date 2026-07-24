using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Users.Requests;

public sealed record UpdateUserPreferencesRequest(UserLanguage Language, UserTheme Theme);
