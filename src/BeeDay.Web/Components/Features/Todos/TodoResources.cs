namespace BeeDay.Web.Components.Features.Todos;

/// <summary>
/// Marker type for resolving the Todos resource catalog via
/// <c>IStringLocalizer&lt;TodoResources&gt;</c>. Covers TodoEditorModal — the only Todos-owned
/// component (to-do cards are rendered by Dashboard's shared ActivityCard, already localized in
/// Sprint 23.5).
/// </summary>
public sealed class TodoResources;
