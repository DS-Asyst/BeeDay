namespace BeeDay.Web.Services.Authentication;

/// <summary>
/// Resolves where an already-authenticated visitor should land when they choose to continue past
/// the public Home, reusing <see cref="LoginDestinationResolver"/>'s profile/onboarding policy
/// instead of re-implementing it. Shared by <c>PublicHeader</c> and the Home page so neither
/// duplicates the profile/onboarding/destination tree.
/// </summary>
public sealed class AuthenticatedEntryDestinationResolver(BeeDayWebService store)
{
    public async Task<string> ResolveAsync()
    {
        var user = await store.GetCurrentUserAsync();
        return LoginDestinationResolver.Resolve(
            hasProfile: user?.HasProfile == true,
            hasCompletedOnboarding: user?.HasCompletedOnboarding == true,
            returnUrl: null);
    }
}
