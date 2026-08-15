namespace BeeDay.Web.Components.Layout;

/// <summary>
/// Marker type for resolving the shared layout/navigation resource catalog via
/// <c>IStringLocalizer&lt;LayoutResources&gt;</c>. Covers the authenticated navigation shell
/// (NavigationItems, DesktopSidebar, MobileHeader, MobileSidebar) and ReconnectModal — chrome
/// reused across every authenticated page, kept out of SharedResources because none of it applies
/// to the public/unauthenticated flows that catalog already serves.
/// </summary>
public sealed class LayoutResources;
