namespace BeeDay.Web.Components.Features.Authentication;

/// <summary>
/// Marker type for resolving the public Authentication pages' resource catalog via
/// <c>IStringLocalizer&lt;AuthenticationResources&gt;</c>. Currently covers Login; other
/// Authentication-area pages (forgot/reset password, email confirmation) get migrated to this
/// same catalog, or their own if the volume/responsibility justifies it, in a later Sprint.
/// </summary>
public sealed class AuthenticationResources;
