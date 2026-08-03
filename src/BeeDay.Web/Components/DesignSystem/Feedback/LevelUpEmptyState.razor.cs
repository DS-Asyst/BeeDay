using BeeDay.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Feedback;

public partial class LevelUpEmptyState
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public PixelIconName? Icon { get; set; }
    [Parameter] public string? Class { get; set; }
}
