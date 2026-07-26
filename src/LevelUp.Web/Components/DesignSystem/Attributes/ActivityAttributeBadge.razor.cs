using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeBadge
{
    [Parameter] public ActivityAttribute? Attribute { get; set; }

    private string AttributeCssName => Attribute?.ToString().ToLowerInvariant() ?? string.Empty;
}
