namespace BeeDay.Web.Components.Features.Habits;

/// <summary>
/// Marker type for resolving the Habits resource catalog via
/// <c>IStringLocalizer&lt;HabitResources&gt;</c>. Covers HabitEditorModal — the only Habits-owned
/// component (HabitCard lives under Dashboard, already localized in Sprint 23.5).
/// </summary>
public sealed class HabitResources;
