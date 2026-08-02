using LevelUp.Domain.Exceptions;

namespace LevelUp.Web.Components.Features.Dashboard.Pages;

public partial class Home : IDisposable
{
    protected override async Task OnInitializedAsync()
    {
        State.Changed += HandleStateChanged;
        await UserInitializer.EnsureInitializedAsync();
        await State.InitializeAsync();

        if (!State.HasProfile)
        {
            // GetDataAsync throws InvalidDomainStateException when the authenticated id no longer
            // resolves to a real User (IDashboardReadService.GetAsync's not-found path) — the same
            // condition the previous LevelUpData-based check expressed as `CurrentUser is null`.
            try
            {
                await State.GetDataAsync();
                Navigation.NavigateTo("/profile/create", forceLoad: true, replace: true);
            }
            catch (InvalidDomainStateException)
            {
                Navigation.NavigateTo("/login", forceLoad: true, replace: true);
            }
        }
    }
    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= HandleStateChanged;
}
