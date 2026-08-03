using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Cards;

public partial class BeeDayCard
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public bool Padded { get; set; }
    [Parameter] public bool Muted { get; set; }
    [Parameter] public bool Interactive { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass => string.Join(' ', new[]
    {
        "beeday-card",
        Padded ? "beeday-card--padded" : null,
        Muted ? "beeday-card--muted" : null,
        Interactive ? "beeday-card--interactive" : null,
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));
}
