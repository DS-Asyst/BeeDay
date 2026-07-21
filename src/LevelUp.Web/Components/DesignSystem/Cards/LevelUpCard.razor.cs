using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Cards;

public partial class LevelUpCard
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => string.IsNullOrWhiteSpace(Class)
        ? "levelup-card"
        : $"levelup-card {Class}";
}
