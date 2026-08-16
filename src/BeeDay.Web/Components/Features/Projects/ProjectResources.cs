namespace BeeDay.Web.Components.Features.Projects;

/// <summary>
/// Marker type for resolving the Projects resource catalog via
/// <c>IStringLocalizer&lt;ProjectResources&gt;</c>. Covers ProjectWorkspace and ProjectEditorModal —
/// the only two Projects-owned components (cards/filters/toasts for Projects live in Dashboard's
/// own DashboardResources, since they're rendered by Dashboard-owned components).
/// </summary>
public sealed class ProjectResources;
