namespace LevelUp.Application.Features.Users.Responses;

/// <summary>
/// Identity-only view of the current User: authentication and account state. Presentation
/// data (name, nickname, avatar, preferences, progress) belongs to Profile, not here.
/// </summary>
public sealed record CurrentUserResponse(Guid Id, string Email, bool IsActive, bool HasCompletedOnboarding, bool IsEmailConfirmed);
