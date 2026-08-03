using BeeDay.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDay.Web.Components.DesignSystem.Buttons;

public partial class BeeDayButton
{
    [Parameter] public BeeDayButtonVariant Variant { get; set; } = BeeDayButtonVariant.Primary;
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public bool Compact { get; set; }
    [Parameter] public PixelIconName? Icon { get; set; }
    [Parameter] public PixelIconSize IconSize { get; set; } = PixelIconSize.Small;
    [Parameter] public string? Class { get; set; }
    [Parameter] public EventCallback OnClick { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClasses
    {
        get
        {
            var variantClass = Variant switch
            {
                BeeDayButtonVariant.Primary => "levelup-button--primary",
                BeeDayButtonVariant.Secondary => "levelup-button--secondary",
                BeeDayButtonVariant.Success => "levelup-button--success",
                BeeDayButtonVariant.Warning => "levelup-button--warning",
                BeeDayButtonVariant.Back => "levelup-button--back",
                BeeDayButtonVariant.Danger => "levelup-button--danger",
                BeeDayButtonVariant.ConfirmationDanger => "levelup-button--confirmation-danger",
                BeeDayButtonVariant.ConfirmationCancel => "levelup-button--confirmation-cancel",
                _ => "levelup-button--primary"
            };

            return string.Join(' ', new[]
            {
                "levelup-button",
                variantClass,
                FullWidth ? "levelup-button--full-width" : null,
                Compact ? "levelup-button--compact" : null,
                Class
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }

    private bool IsDisabled => Disabled || IsLoading;

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled || !OnClick.HasDelegate)
        {
            return;
        }

        await OnClick.InvokeAsync(args);
    }
}
