namespace BeeDay.Web.Components.Features.Home;

/// <summary>
/// Marker type for resolving the public Home page's own resource catalog via
/// <c>IStringLocalizer&lt;HomeResources&gt;</c>. Text shared with other public-chrome components
/// (e.g. the "Continue to BeeDay" CTA also shown by <c>PublicHeader</c>) lives in
/// <c>BeeDay.Web.Resources.SharedResources</c> instead — this catalog is only for copy that
/// belongs to the Home page itself.
/// </summary>
public sealed class HomeResources;
