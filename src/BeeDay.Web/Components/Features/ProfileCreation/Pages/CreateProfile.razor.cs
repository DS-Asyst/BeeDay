using BeeDay.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BeeDay.Web.Components.Features.ProfileCreation.Pages;

public partial class CreateProfile
{
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] private AuthenticatedUserInitializer UserInitializer { get; set; } = default!;

    private bool _hasAuthenticatedSession;

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        _hasAuthenticatedSession = authenticationState.User.Identity?.IsAuthenticated == true;

        if (_hasAuthenticatedSession)
        {
            await UserInitializer.EnsureInitializedAsync();
        }

        var status = await State.InitializeAsync(_hasAuthenticatedSession);

        if (_hasAuthenticatedSession && status.HasProfile)
        {
            var user = await State.LoadDataAsync();
            var destination = user?.HasCompletedOnboarding == true
                ? "/daily"
                : "/onboarding/tutorial";
            Navigation.NavigateTo(destination, forceLoad: true, replace: true);
        }
    }

    private void ContinueToProfile() => State.ContinueToProfile();
    private void Back() => State.Back();
    private void BackToLogin() => Navigation.NavigateTo("/login");

    private async Task CompleteProfileAsync()
    {
        if (!await State.CompleteProfileAsync())
        {
            return;
        }

        if (_hasAuthenticatedSession)
        {
            Navigation.NavigateTo("/onboarding/tutorial", forceLoad: true, replace: true);
            return;
        }

        Navigation.NavigateTo(
            $"/account/email-confirmation-sent?email={Uri.EscapeDataString(State.Model.Email.Trim())}",
            forceLoad: true,
            replace: true);
    }
}
