using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeSelect
{
    private static readonly ActivityAttribute[] Attributes = Enum.GetValues<ActivityAttribute>();
    private bool isOpen;

    [Parameter, EditorRequired] public string Id { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public string FieldCssClass { get; set; } = "levelup-field";
    [Parameter] public string LabelCssClass { get; set; } = "levelup-field__label";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public ActivityAttribute? Value { get; set; }
    [Parameter] public EventCallback<ActivityAttribute?> ValueChanged { get; set; }

    private void ToggleMenu()
    {
        if (Disabled)
        {
            return;
        }

        isOpen = !isOpen;
    }

    private Task SelectAsync(ActivityAttribute? attribute)
    {
        isOpen = false;
        return ValueChanged.InvokeAsync(attribute);
    }
}
