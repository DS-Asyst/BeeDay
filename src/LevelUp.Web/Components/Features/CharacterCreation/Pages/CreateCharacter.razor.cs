using LevelUp.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace LevelUp.Web.Components.Features.CharacterCreation.Pages;

public partial class CreateCharacter
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

        if (_hasAuthenticatedSession && status.HasCharacter)
        {
            var data = await State.LoadDataAsync();
            var destination = data.CurrentUser?.HasCompletedOnboarding == true
                ? "/daily"
                : "/onboarding/tutorial";
            Navigation.NavigateTo(destination, forceLoad: true, replace: true);
        }
    }

    private void ContinueToCharacter() => State.ContinueToCharacter();
    private void ContinueToClasses() => State.ContinueToClasses();
    private void Back() => State.Back();
    private void BackToLogin() => Navigation.NavigateTo("/login");

    private void RequestClassConfirmation(
        LevelUp.Web.Components.Features.CharacterCreation.State.CharacterClassOption option) =>
        State.RequestClassConfirmation(option);

    private void CloseConfirmation() => State.CloseConfirmation();

    private async Task ConfirmClassAsync()
    {
        if (!await State.ConfirmClassAsync())
        {
            return;
        }

        if (_hasAuthenticatedSession)
        {
            Navigation.NavigateTo("/onboarding/tutorial", forceLoad: true, replace: true);
            return;
        }

        Navigation.NavigateTo(
            $"/login?registered=true&email={Uri.EscapeDataString(State.Model.Email.Trim())}",
            forceLoad: true,
            replace: true);
    }
}
