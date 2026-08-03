using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Users.Responses;

/// <summary>
/// Identity and account-settings view of the current User — <c>Name</c>, <c>Nickname</c>,
/// <c>Language</c>, <c>Theme</c> and <c>HasProfile</c> were added on the Sprint 14.6 cutover so the 4 Web
/// consumers that used to call <c>ILevelUpRepository.LoadAsync()</c> for a profile-completion gate or an
/// account-settings form (<c>Account.razor</c>, <c>Tutorial.razor</c>, <c>Entry.razor</c>,
/// <c>ProfileCreationState</c>) could stop depending on <c>LevelUpData</c>/<c>GetLevelUpQuery</c>. Does
/// not carry Avatar or XP/level progress — no current consumer of this response needs them, and they
/// belong to the Dashboard projection (<c>UserProfileSummary</c>) if a future consumer does.
/// </summary>
public sealed record CurrentUserResponse(
    Guid Id,
    string Name,
    string Email,
    string Nickname,
    UserLanguage Language,
    UserTheme Theme,
    bool IsActive,
    bool HasCompletedOnboarding,
    bool IsEmailConfirmed,
    bool HasProfile);
