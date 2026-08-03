using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Icons;

public partial class PixelIcon
{
    [Parameter, EditorRequired] public PixelIconName Name { get; set; }
    [Parameter] public PixelIconSize Size { get; set; } = PixelIconSize.Medium;
    [Parameter] public PixelIconColor Color { get; set; } = PixelIconColor.Current;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private PixelIconDefinition Definition => PixelIconRegistry.Resolve(Name);
    private string SpriteReference => $"{PixelIconRegistry.SpritePath}#{Definition.SymbolId}";
    private int PixelSize => Size switch
    {
        PixelIconSize.ExtraSmall => 12,
        PixelIconSize.Small => 16,
        PixelIconSize.Medium => 20,
        PixelIconSize.Large => 24,
        PixelIconSize.ExtraLarge => 32,
        _ => 20
    };

    private string CssClasses => string.Join(' ', new[]
    {
        "pixel-icon",
        $"pixel-icon--{Definition.SymbolId}",
        $"pixel-icon--size-{Size.ToString().ToLowerInvariant()}",
        $"pixel-icon--color-{Color.ToString().ToLowerInvariant()}",
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    protected override void OnParametersSet()
    {
        if (!Decorative && string.IsNullOrWhiteSpace(Label))
        {
            throw new InvalidOperationException(
                $"{nameof(PixelIcon)} requires a non-empty {nameof(Label)} when {nameof(Decorative)} is false.");
        }
    }
}
