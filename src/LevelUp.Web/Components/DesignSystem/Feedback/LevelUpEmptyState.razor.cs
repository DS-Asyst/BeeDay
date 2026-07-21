using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Feedback;

public partial class LevelUpEmptyState
{
    [Parameter, EditorRequired] public string Message { get; set; } = string.Empty;
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string? Class { get; set; }
}
