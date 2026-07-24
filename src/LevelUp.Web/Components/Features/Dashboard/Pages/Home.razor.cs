namespace LevelUp.Web.Components.Features.Dashboard.Pages;

public partial class Home : IDisposable
{
    protected override async Task OnInitializedAsync()
    {
        State.Changed += HandleStateChanged;
        await UserInitializer.EnsureInitializedAsync();
        await State.InitializeAsync();

        if (!State.HasCharacter)
        {
            var data = await State.GetDataAsync();
            Navigation.NavigateTo(
                data.CurrentUser is null ? "/login" : "/character/create",
                forceLoad: true,
                replace: true);
        }
    }
    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= HandleStateChanged;
}
