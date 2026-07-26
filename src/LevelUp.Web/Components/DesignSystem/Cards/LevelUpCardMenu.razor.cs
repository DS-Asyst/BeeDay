using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Cards;

public partial class LevelUpCardMenu
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string TriggerClass { get; set; } = string.Empty;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    private bool isOpen;
    private string AriaLabel => $"Options for {Title}";
    private string MenuCssClass => string.IsNullOrWhiteSpace(Class)
        ? "card-action-menu"
        : $"card-action-menu {Class}";
    private string TriggerCssClass => TriggerClass;

    private async Task ToggleMenu()
    {
        if (Disabled)
        {
            return;
        }

        isOpen = !isOpen;
        await OpenChanged.InvokeAsync(isOpen);
    }

    private async Task CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        await OpenChanged.InvokeAsync(false);
    }

    private async Task EditAsync()
    {
        await CloseMenu();
        await OnEdit.InvokeAsync();
    }

    private async Task DeleteAsync()
    {
        await CloseMenu();
        await OnDelete.InvokeAsync();
    }
}
