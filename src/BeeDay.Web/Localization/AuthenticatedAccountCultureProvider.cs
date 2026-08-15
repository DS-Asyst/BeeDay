using BeeDay.Domain.Entities;
using Microsoft.AspNetCore.Localization;

namespace BeeDay.Web.Localization;

/// <summary>
/// Resolves the effective culture for an authenticated request that carries no explicit
/// BeeDay.Culture cookie yet, from the account's persisted UserLanguage — closing the one gap
/// AuthenticatedCultureSynchronizer.SynchronizeAtLoginAsync structurally cannot: a session
/// authenticated purely by the persistent "BeeDay.Auth" cookie (e.g. reopening the browser days
/// later) never repasses through <c>POST /auth/login</c>, the only other place this precedence
/// rule is applied.
///
/// Registered in <c>RequestLocalizationOptions.RequestCultureProviders</c> immediately after
/// <see cref="CookieRequestCultureProvider"/>, so an explicit BeeDay.Culture cookie always wins —
/// this provider is never even consulted when one exists — and only ever runs where the framework
/// itself already applies its result, avoiding the ambient-culture timing gap a per-request
/// authentication event handler running after <c>UseRequestLocalization()</c> cannot close on its
/// own.
///
/// Reads the User that <c>OnValidatePrincipal</c> (see Program.cs) already loaded for its own
/// mandatory session/SessionVersion validation, handed off via <see cref="HttpContextItemsKey"/> —
/// never re-reads it. Requires <c>UseAuthentication()</c> to run before
/// <c>UseRequestLocalization()</c> so that hand-off has already happened by the time this
/// provider runs; for an anonymous request (or one whose cookie failed validation),
/// <c>HttpContext.Items</c> carries nothing here and this provider defers to the fallback culture,
/// unchanged from before this Sprint.
/// </summary>
public sealed class AuthenticatedAccountCultureProvider(bool isDevelopment) : IRequestCultureProvider
{
    public static readonly object HttpContextItemsKey = new();

    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext.Items[HttpContextItemsKey] is not User user)
        {
            return Task.FromResult<ProviderCultureResult?>(null);
        }

        var culture = BeeDayCultures.FromUserLanguage(user.Language);

        // Written once here so this same lookup is not repeated on the visitor's next request —
        // from then on, CookieRequestCultureProvider (tried first) resolves it directly.
        httpContext.Response.Cookies.Append(
            BeeDayCultures.CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            BeeDayCultures.CreateCookieOptions(isDevelopment));

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
    }
}
