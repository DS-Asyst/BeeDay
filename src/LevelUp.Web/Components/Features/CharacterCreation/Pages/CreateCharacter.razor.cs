namespace LevelUp.Web.Components.Features.CharacterCreation.Pages;

public partial class CreateCharacter
{
    protected override async Task OnInitializedAsync()
    {
        var status = await State.InitializeAsync();

        if (status.HasCharacter)
        {
            Navigation.NavigateTo("/daily", forceLoad: true, replace: true);
        }
    }

    private void ContinueToCharacter()
    {
        State.ContinueToCharacter();
    }

    private void ContinueToClasses()
    {
        State.ContinueToClasses();
    }

    private void Back()
    {
        State.Back();
    }

    private void RequestClassConfirmation(
        LevelUp.Web.Components.Features.CharacterCreation.State.CharacterClassOption option)
    {
        State.RequestClassConfirmation(option);
    }

    private void CloseConfirmation()
    {
        State.CloseConfirmation();
    }

    private async Task ConfirmClassAsync()
    {
        if (!await State.ConfirmClassAsync())
        {
            return;
        }

        var status = await State.GetStatusAsync();

        if (status.HasUser && status.HasCharacter)
        {
            Navigation.NavigateTo("/daily", forceLoad: true, replace: true);
        }
    }
}
