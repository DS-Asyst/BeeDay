using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDay.Web.Components.Layout;

public partial class MobileSidebar
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public bool IsProfilePanelOpen { get; set; }
    [Parameter] public bool IsMenuPanelOpen { get; set; }
    [Parameter] public EventCallback OnToggleProfilePanel { get; set; }
    [Parameter] public EventCallback OnToggleMenuPanel { get; set; }

    private ElementReference closeButtonElement;
    private bool wasOpen;

    private string AriaHiddenValue => IsOpen ? "false" : "true";

    private Task HandleNavigate() => OnClose.InvokeAsync();

    private Task HandleKeyDown(KeyboardEventArgs args) =>
        args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !wasOpen)
        {
            await closeButtonElement.FocusAsync();
        }

        wasOpen = IsOpen;
    }
}
