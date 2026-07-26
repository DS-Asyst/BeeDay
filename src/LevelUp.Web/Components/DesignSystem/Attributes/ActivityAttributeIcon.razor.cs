using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeIcon
{
    [Parameter] public ActivityAttribute? Attribute { get; set; }
    [Parameter] public int Size { get; set; } = 16;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Class { get; set; }

    private string Label => Attribute?.ToString() ?? string.Empty;
    private string IconPath => $"/icons/pixel/attribute-{Attribute?.ToString().ToLowerInvariant()}.svg";
}
