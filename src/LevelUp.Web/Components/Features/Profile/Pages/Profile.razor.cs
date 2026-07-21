namespace LevelUp.Web.Components.Features.Profile.Pages;

public partial class Profile
{
    protected override async Task OnInitializedAsync()
    {
        if (await State.ProfileAlreadyExistsAsync())
        {
            Navigation.NavigateTo("/", replace: true);
        }
    }

    private async Task ConfirmClassAsync()
    {
        if (await State.ConfirmClassAsync())
        {
            Navigation.NavigateTo("/", forceLoad: true);
        }
    }
}
