using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Services;

public sealed class NavigationService
{
    private readonly NavigationManager _navigationManager;

    public NavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NavigateTo(string uri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri);

        _navigationManager.NavigateTo(uri);
    }

    public void NavigateToHome()
    {
        _navigationManager.NavigateTo("/");
    }
}
