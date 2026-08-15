using BeeDay.Domain.Enums;

namespace BeeDay.Web.Localization;

/// <summary>
/// Single source of truth for which cultures BeeDay's UI supports, which one is the safe
/// fallback, the name of the cookie that persists the effective culture across requests, and the
/// mapping between the Domain's <see cref="UserLanguage"/> enum and Web culture codes.
/// </summary>
public static class BeeDayCultures
{
    public const string English = "en-US";
    public const string Portuguese = "pt-BR";
    public const string Default = English;

    /// <summary>
    /// Name of the cookie that persists the effective UI culture. Deliberately separate from the
    /// "BeeDay.Auth" authentication cookie — culture is a presentation preference, not a
    /// credential, and must remain readable/settable independently of sign-in state.
    /// </summary>
    public const string CookieName = "BeeDay.Culture";

    public static readonly string[] Supported = [English, Portuguese];

    public static string FromUserLanguage(UserLanguage language) => language switch
    {
        UserLanguage.Portuguese => Portuguese,
        _ => English
    };

    public static UserLanguage ToUserLanguage(string culture) => culture switch
    {
        Portuguese => UserLanguage.Portuguese,
        _ => UserLanguage.English
    };

    /// <summary>
    /// Attributes shared by every place that writes the culture cookie (the public /culture/set
    /// endpoint and the authenticated-login synchronizer) — kept in one place so they can never
    /// drift apart.
    /// </summary>
    public static CookieOptions CreateCookieOptions(bool isDevelopment) => new()
    {
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        Secure = !isDevelopment,
        Expires = DateTimeOffset.UtcNow.AddYears(1)
    };
}
