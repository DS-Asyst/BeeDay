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
            var data = await State.GetDataAsync();
            Navigation.NavigateTo(
                data.CurrentUser is null ? "/login" : "/profile/create",
                forceLoad: true,
                replace: true);
        }
    }
    private void HandleStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => State.Changed -= HandleStateChanged;
}
