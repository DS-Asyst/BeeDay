using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Users.Responses;

public sealed record CurrentUserResponse(Guid Id, string Name, string Email, UserLanguage Language, UserTheme Theme, bool IsActive, bool HasCompletedOnboarding, bool IsEmailConfirmed);
