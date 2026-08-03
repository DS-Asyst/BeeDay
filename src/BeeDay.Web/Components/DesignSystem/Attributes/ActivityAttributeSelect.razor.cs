using System.Globalization;
using BeeDay.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BeeDay.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeSelect : IAsyncDisposable
{
    // Matches the Design System's --beeday-spacing-sm token (.5rem @ 16px
    // root = 8px). Kept as a plain pixel constant because the placement math
    // (AttributeSelectPlacementCalculator) operates on real
    // getBoundingClientRect numbers, not CSS custom properties.
    private const double ViewportMarginPx = 8;
    private const double GapPx = 6;

    private static readonly ActivityAttribute[] Attributes = Enum.GetValues<ActivityAttribute>();

    private ElementReference triggerElement;
    private ElementReference menuElement;
    private IJSObjectReference? module;
    private bool isOpen;
    private bool isMeasuring;
    private bool flipUp;
    private double topPx;
    private double leftPx;
    private double widthPx;
    private double maxHeightPx;

    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "beeday-field";
    [Parameter] public string LabelCssClass { get; set; } = "beeday-field__label";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public ActivityAttribute? Value { get; set; }
    [Parameter] public EventCallback<ActivityAttribute?> ValueChanged { get; set; }

    private string MenuStyle => string.Create(CultureInfo.InvariantCulture,
        $"top:{topPx}px;left:{leftPx}px;width:{widthPx}px;max-height:{maxHeightPx}px;");

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (isOpen && isMeasuring)
        {
            await MeasureAndApplyPlacementAsync();
        }
    }

    private void ToggleMenu()
    {
        if (Disabled)
        {
            return;
        }

        if (isOpen)
        {
            isOpen = false;
            return;
        }

        // Open immediately in a hidden "measuring" state — the real menu
        // markup renders and lays out normally (at the trigger's own
        // position/width) so its actual height can be measured next render,
        // instead of guessing.
        flipUp = false;
        isMeasuring = true;
        isOpen = true;
    }

    private Task SelectAsync(ActivityAttribute? attribute)
    {
        isOpen = false;
        return ValueChanged.InvokeAsync(attribute);
    }

    private async Task<IJSObjectReference> EnsureModuleAsync() =>
        module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./js/activity-attribute-select.js");

    private async Task MeasureAndApplyPlacementAsync()
    {
        try
        {
            var jsModule = await EnsureModuleAsync();
            var geometry = await jsModule.InvokeAsync<AttributeSelectGeometry>("measureGeometry", triggerElement, menuElement);
            var placement = AttributeSelectPlacementCalculator.Calculate(geometry, ViewportMarginPx, GapPx);

            flipUp = placement.FlipUp;
            topPx = placement.TopPx;
            leftPx = geometry.TriggerLeft;
            widthPx = geometry.TriggerWidth;
            maxHeightPx = placement.MaxHeightPx;
        }
        catch (JSException)
        {
            flipUp = false;
        }
        finally
        {
            isMeasuring = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already disconnected.
            }
        }
    }
}
