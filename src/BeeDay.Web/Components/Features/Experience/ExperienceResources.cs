namespace BeeDay.Web.Components.Features.Experience;

/// <summary>
/// Marker type for resolving the Experience resource catalog via
/// <c>IStringLocalizer&lt;ExperienceResources&gt;</c>. Covers ExperienceBar and the level-up
/// feedback flow (BeeDayFeedbackModal, BeeDayFeedback's formerly hardcoded display strings) —
/// kept separate from DashboardResources since Experience has its own responsibility and enough
/// string volume to justify its own catalog, even though it is currently only consumed by the
/// Dashboard feature.
/// </summary>
public sealed class ExperienceResources;
