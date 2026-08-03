using BeeDay.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Attributes;

public partial class ActivityAttributeBadge
{
    [Parameter] public ActivityAttribute? Attribute { get; set; }

    private string AttributeCssName => Attribute?.ToString().ToLowerInvariant() ?? string.Empty;
    private string Tooltip => Attribute is null ? string.Empty : $"{Attribute} activity attribute";
}
