namespace BeeDay.Web.Services;

/// <summary>
/// Coordinates BeeDayCardMenu instances within a single circuit so opening one
/// card's action menu closes any other menu already open elsewhere on the page.
/// </summary>
public sealed class CardActionMenuCoordinator
{
    public event Action<Guid>? MenuOpened;

    public void NotifyOpened(Guid menuId) => MenuOpened?.Invoke(menuId);
}
