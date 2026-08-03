using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BeeDay.Web.Components.DesignSystem.Cards;

public partial class BeeDayCardMenu : IAsyncDisposable
{
    // Matches the Design System's --levelup-spacing-sm token (.5rem @ 16px
    // root = 8px). Kept as a plain pixel constant here because the placement
    // math (CardMenuPlacementCalculator) operates on real getBoundingClientRect
    // numbers, not CSS custom properties.
    private const double ViewportMarginPx = 8;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string TriggerClass { get; set; } = string.Empty;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    private readonly Guid menuId = Guid.NewGuid();
    private ElementReference rootElement;
    private ElementReference triggerElement;
    private ElementReference panelElement;
    private IJSObjectReference? module;
    private DotNetObjectReference<BeeDayCardMenu>? selfReference;
    private bool isOpen;
    private bool isMeasuring;
    private bool flipUp;
    private double horizontalShiftPx;

    private string AriaLabel => $"Options for {Title}";
    private string MenuCssClass => string.IsNullOrWhiteSpace(Class)
        ? "card-action-menu"
        : $"card-action-menu {Class}";
    private string TriggerCssClass => TriggerClass;

    protected override void OnInitialized() =>
        Coordinator.MenuOpened += HandleAnotherMenuOpened;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (isOpen && isMeasuring)
        {
            await MeasureAndApplyPlacementAsync();
        }
    }

    private void HandleAnotherMenuOpened(Guid openedMenuId)
    {
        if (!isOpen || openedMenuId == menuId)
        {
            return;
        }

        isOpen = false;
        _ = InvokeAsync(async () =>
        {
            await UnregisterOutsideClickAsync();
            StateHasChanged();
            await OpenChanged.InvokeAsync(false);
        });
    }

    private async Task ToggleMenu()
    {
        if (Disabled)
        {
            return;
        }

        if (isOpen)
        {
            await CloseMenu();
            return;
        }

        // Open immediately in a hidden "measuring" state — the real panel
        // markup (Edit/Delete) renders and lays out normally so its actual
        // width/height can be measured next render, instead of guessing.
        flipUp = false;
        horizontalShiftPx = 0;
        isMeasuring = true;
        isOpen = true;
        Coordinator.NotifyOpened(menuId);
        await OpenChanged.InvokeAsync(true);
    }

    private async Task CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        isMeasuring = false;
        await UnregisterOutsideClickAsync();
        await OpenChanged.InvokeAsync(false);
    }

    private Task HandleKeyDown(KeyboardEventArgs e) =>
        e.Key == "Escape" ? CloseMenu() : Task.CompletedTask;

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

    [JSInvokable]
    public Task NotifyOutsideClickAsync() => CloseMenu();

    private async Task<IJSObjectReference> EnsureModuleAsync() =>
        module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/beeday-card-menu.js?v=20260729-1");

    private async Task MeasureAndApplyPlacementAsync()
    {
        try
        {
            var jsModule = await EnsureModuleAsync();
            var geometry = await jsModule.InvokeAsync<CardMenuGeometry>("measureGeometry", triggerElement, panelElement);
            var placement = CardMenuPlacementCalculator.Calculate(geometry, ViewportMarginPx);
            flipUp = placement.FlipUp;
            horizontalShiftPx = placement.HorizontalShiftPx;
        }
        catch (JSException)
        {
            flipUp = false;
            horizontalShiftPx = 0;
        }
        finally
        {
            isMeasuring = false;
            StateHasChanged();
            await RegisterOutsideClickAsync();
        }
    }

    private async Task RegisterOutsideClickAsync()
    {
        try
        {
            var jsModule = await EnsureModuleAsync();
            selfReference ??= DotNetObjectReference.Create(this);
            await jsModule.InvokeVoidAsync("registerOutsideClick", rootElement, selfReference);
        }
        catch (JSException)
        {
            // Outside-click detection is a progressive enhancement; the menu
            // still works via the trigger, Escape, Edit, and Delete.
        }
    }

    private async Task UnregisterOutsideClickAsync()
    {
        if (module is null)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("unregisterOutsideClick", rootElement);
        }
        catch (JSException)
        {
        }
        catch (JSDisconnectedException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        Coordinator.MenuOpened -= HandleAnotherMenuOpened;

        if (module is not null)
        {
            try
            {
                if (isOpen)
                {
                    await module.InvokeVoidAsync("unregisterOutsideClick", rootElement);
                }

                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already disconnected.
            }
        }

        selfReference?.Dispose();
    }
}
