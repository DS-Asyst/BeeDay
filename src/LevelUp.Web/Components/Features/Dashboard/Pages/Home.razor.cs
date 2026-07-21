namespace LevelUp.Web.Components.Features.Dashboard.Pages;

public partial class Home : IDisposable
{
    protected override async Task OnInitializedAsync()
    {
        State.Changed += HandleStateChanged;
        await State.InitializeAsync();

        if (!State.HasProfile)
        {
            Navigation.NavigateTo("/profile", replace: true);
        }
    }
    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= HandleStateChanged;
}
