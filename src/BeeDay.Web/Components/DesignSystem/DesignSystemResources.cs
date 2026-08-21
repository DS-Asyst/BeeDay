namespace BeeDay.Web.Components.DesignSystem;

/// <summary>
/// Marker type for resolving the Design System's shared default-copy resource catalog via
/// <c>IStringLocalizer&lt;DesignSystemResources&gt;</c>. Covers the culture-aware defaults for
/// reusable components (BeeDayConfirmDialog, BeeDayLoading, EditorModalShell,
/// BeeDayProgressBar) — every consumer's explicit overrides remain feature-specific copy and stay
/// out of this catalog.
/// </summary>
public sealed class DesignSystemResources;
