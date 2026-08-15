namespace BeeDay.Web.Components.Features.Account;

/// <summary>
/// Marker type for resolving the Account/Settings resource catalog via
/// <c>IStringLocalizer&lt;AccountResources&gt;</c>. Covers Account.razor and its three sections
/// (Profile, Security, Preferences) — one catalog for the whole feature, since none of these
/// strings are shared with any other area of the app.
/// </summary>
public sealed class AccountResources;
