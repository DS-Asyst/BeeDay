namespace BeeDay.Web.Services.Authentication;

public static class LoginDestinationResolver
{
    public static string Resolve(
        bool hasProfile,
        bool hasCompletedOnboarding,
        string? returnUrl)
    {
        if (!hasProfile)
        {
            return "/profile/create";
        }

        if (!hasCompletedOnboarding)
        {
            return "/onboarding/tutorial";
        }

        return IsLocalPath(returnUrl) ? returnUrl! : "/home";
    }

    public static string ResolveLogout(string? returnUrl) =>
        IsLocalPath(returnUrl) ? returnUrl! : "/login";

    internal static bool IsLocalPath(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.StartsWith("/", StringComparison.Ordinal) &&
        !value.StartsWith("//", StringComparison.Ordinal) &&
        !value.StartsWith("/\\", StringComparison.Ordinal);
}
