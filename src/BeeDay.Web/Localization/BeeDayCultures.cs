namespace BeeDay.Web.Localization;

/// <summary>
/// Single source of truth for which cultures BeeDay's UI supports, which one is the safe
/// fallback, and the name of the cookie that persists the effective culture across requests.
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
}
