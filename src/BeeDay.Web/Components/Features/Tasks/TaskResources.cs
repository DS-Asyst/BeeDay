namespace BeeDay.Web.Components.Features.Tasks;

/// <summary>
/// Marker type for resolving the Tasks resource catalog via
/// <c>IStringLocalizer&lt;TaskResources&gt;</c>. Covers TaskEditorModal — the only Tasks-owned
/// component (task cards are rendered by Dashboard's shared ActivityCard, already localized in
/// Sprint 23.5).
/// </summary>
public sealed class TaskResources;
