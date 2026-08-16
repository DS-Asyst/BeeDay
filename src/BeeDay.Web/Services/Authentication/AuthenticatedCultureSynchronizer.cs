using BeeDay.Application.Features.Users.Commands;
using BeeDay.Application.Features.Users.Queries;
using BeeDay.Application.Features.Users.Requests;
using BeeDay.Web.Localization;
using MediatR;
using Microsoft.AspNetCore.Localization;

namespace BeeDay.Web.Services.Authentication;

/// <summary>
/// Applies the precedence rule Epic 23 defines for a fresh login:
///
/// <code>
/// 1. the culture explicitly chosen this session (the BeeDay.Culture cookie)
/// 2. the account's persisted UserLanguage
/// 3. the application fallback (BeeDayCultures.Default)
/// </code>
///
/// When there is no explicit cookie yet, the account preference becomes the effective culture.
/// When an explicit cookie disagrees with the account, the cookie wins for this session and the
/// account converges to it so future sessions already start in sync.
///
/// Runs once per login, so it can afford to go through the same Application-layer commands
/// Settings itself uses (<see cref="GetCurrentUserQuery"/>, <see cref="UpdateCurrentUserPreferencesCommand"/>)
/// rather than touching IUserRepository directly — <c>/auth/login</c> assigns
/// <c>HttpContext.User</c> right after SignInAsync specifically so ICurrentUserContext already
/// resolves the just-authenticated user here.
///
/// The complementary case — an already-authenticated session that never repasses through
/// <c>POST /auth/login</c> (a persistent "remember me" cookie) — is handled separately and far
/// more cheaply by <see cref="BeeDay.Web.Localization.AuthenticatedAccountCultureProvider"/>,
/// registered directly into the request-localization pipeline; it is read-only and never
/// converges the account, so it does not belong in this login-scoped synchronizer.
/// </summary>
public sealed class AuthenticatedCultureSynchronizer(ISender sender, IWebHostEnvironment environment)
{
    public async Task<string> SynchronizeAtLoginAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
        if (user is null)
        {
            return BeeDayCultures.Default;
        }

        var accountCulture = BeeDayCultures.FromUserLanguage(user.Language);
        var explicitCulture = TryGetExplicitCulture(httpContext);

        if (explicitCulture is null)
        {
            SetCultureCookie(httpContext, accountCulture);
            return accountCulture;
        }

        if (!string.Equals(explicitCulture, accountCulture, StringComparison.OrdinalIgnoreCase))
        {
            var convergedLanguage = BeeDayCultures.ToUserLanguage(explicitCulture);
            await sender.Send(
                new UpdateCurrentUserPreferencesCommand(new UpdateUserPreferencesRequest(convergedLanguage, user.Theme)),
                cancellationToken);
        }

        return explicitCulture;
    }

    private static string? TryGetExplicitCulture(HttpContext httpContext)
    {
        var raw = httpContext.Request.Cookies[BeeDayCultures.CookieName];
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        var parsed = CookieRequestCultureProvider.ParseCookieValue(raw);
        var culture = parsed is { Cultures.Count: > 0 } ? parsed.Cultures[0].Value : null;

        return culture is not null && BeeDayCultures.Supported.Contains(culture, StringComparer.OrdinalIgnoreCase)
            ? culture
            : null;
    }

    private void SetCultureCookie(HttpContext httpContext, string culture) =>
        httpContext.Response.Cookies.Append(
            BeeDayCultures.CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            BeeDayCultures.CreateCookieOptions(environment.IsDevelopment()));
}
