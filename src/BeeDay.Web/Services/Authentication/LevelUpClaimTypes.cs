namespace BeeDay.Web.Services.Authentication;

public static class LevelUpClaimTypes
{
    /// <summary>
    /// Carries the User's <c>SessionVersion</c> at the time the cookie was issued. Compared
    /// against the current value on every request so password changes, resets, and account
    /// deactivation invalidate already-issued cookies without a persisted session store.
    /// </summary>
    public const string SessionVersion = "levelup:session_version";
}
