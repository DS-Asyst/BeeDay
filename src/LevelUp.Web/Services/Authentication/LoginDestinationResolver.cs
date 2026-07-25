namespace LevelUp.Web.Services.Authentication;

public static class LoginDestinationResolver
{
    public static string Resolve(
        bool hasCharacter,
        bool hasCompletedOnboarding,
        string? returnUrl)
    {
        if (!hasCharacter)
        {
            return "/character/create";
        }

        if (!hasCompletedOnboarding)
        {
            return "/onboarding/tutorial";
        }

        return IsLocalPath(returnUrl) ? returnUrl! : "/daily";
    }

    internal static bool IsLocalPath(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.StartsWith("/", StringComparison.Ordinal) &&
        !value.StartsWith("//", StringComparison.Ordinal) &&
        !value.StartsWith("/\\", StringComparison.Ordinal);
}
