using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Experience.Feedback;

public sealed record LevelUpFeedback(
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
