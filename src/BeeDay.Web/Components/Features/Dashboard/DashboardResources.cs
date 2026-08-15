namespace BeeDay.Web.Components.Features.Dashboard;

/// <summary>
/// Marker type for resolving the Dashboard resource catalog via
/// <c>IStringLocalizer&lt;DashboardResources&gt;</c>. Covers both Dashboard pages (/daily, /profile),
/// their shared components (ActivityFilterBar, DashboardColumn, ActivityCard, HabitCard,
/// ProjectContextFilter), DashboardState's feedback messages, and BeeDayDashboardSkeleton's
/// Dashboard-owned aria-label — one catalog for the whole feature.
/// </summary>
public sealed class DashboardResources;
