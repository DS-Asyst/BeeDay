using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Experience.Feedback;

/// <summary>
/// ExperienceSummary/HistorySummary are public members of this record and stay for backward
/// compatibility, unchanged (still English-only — a record has no access to IStringLocalizer at
/// construction time). BeeDayFeedbackModal, the only actual renderer of this data, does not use
/// them: it computes its own culture-aware equivalents via IStringLocalizer&lt;ExperienceResources&gt;,
/// keeping localization responsibility in the presentation layer without removing this API.
/// </summary>
public sealed record BeeDayFeedback(
    Guid EventId,
    Guid ExperienceEntryId,
    int PreviousLevel,
    int NewLevel,
    int LevelsGained,
    long ExperienceAmount,
    ExperienceSourceType ExperienceSource,
    DateTimeOffset OccurredAtUtc)
{
    public string ExperienceSummary => $"+{ExperienceAmount} XP from {FormatSource(ExperienceSource)}";

    public string HistorySummary => $"Reached Level {NewLevel}";

    private static string FormatSource(ExperienceSourceType source) => source switch
    {
        ExperienceSourceType.Habit => "Habit Completed",
        ExperienceSourceType.Task => "Task Completed",
        ExperienceSourceType.Todo => "To-Do Completed",
        ExperienceSourceType.Project => "Project Completed",
        _ => source.ToString(),
    };
}
