using LevelUp.Application.Features.Wallets.Responses;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Features.Dashboard.Responses;

/// <summary>
/// Replaced <c>GetLevelUpResponse</c> (removed on the Sprint 14.6 cutover), which exposed
/// <c>LevelUpData</c> directly — the bloqueador documented in docs/architecture/01-current-state.md
/// §3.6. Composed entirely of read-only projections; never a mutable Domain entity or the document root.
/// </summary>
public sealed record DashboardResponse(
    UserProfileSummary Profile,
    IReadOnlyList<HabitSummary> Habits,
    IReadOnlyList<TaskSummary> Tasks,
    IReadOnlyList<ProjectSummary> Projects,
    WalletSummaryResponse? Wallet);

public sealed record UserProfileSummary(
    Guid UserId,
    string Nickname,
    string Name,
    string Avatar,
    UserLanguage Language,
    UserTheme Theme,
    long TotalExperience,
    int CurrentLevel,
    long CurrentLevelExperience,
    long ExperienceRequiredForCurrentLevel)
{
    public bool HasProfile => !string.IsNullOrEmpty(Nickname);
}

public sealed record HabitSummary(
    Guid Id,
    string Title,
    string Description,
    bool Featured,
    ActivityAttribute? Attribute,
    HabitDirection Direction,
    HabitDifficulty Difficulty,
    HabitResetCounter ResetCounter,
    int PositiveCount,
    int NegativeCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record TaskSummary(
    Guid Id,
    string Title,
    string Description,
    bool Featured,
    ActivityAttribute? Attribute,
    TaskRepeat Repeat,
    bool Completed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record ProjectSummary(
    Guid Id,
    string Name,
    string Description,
    string Color,
    bool Featured,
    ActivityAttribute? Attribute,
    DateOnly? ExpectedDate,
    bool Archived,
    ProjectStatus Status,
    decimal ProgressPercentage,
    IReadOnlyList<TodoSummary> Todos)
{
    public bool Completed => Status == ProjectStatus.Completed;
}

public sealed record TodoSummary(
    Guid Id,
    string Title,
    string Description,
    Guid ProjectId,
    bool Featured,
    DateOnly? DueDate,
    ActivityAttribute? Attribute,
    bool Completed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
