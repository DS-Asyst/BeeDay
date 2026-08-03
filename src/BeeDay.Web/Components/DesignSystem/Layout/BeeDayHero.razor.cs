using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.DesignSystem.Layout;

public partial class BeeDayHero
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Eyebrow { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? Illustration { get; set; }
    [Parameter] public RenderFragment? PrimaryAction { get; set; }
    [Parameter] public RenderFragment? SupportingContent { get; set; }
    [Parameter] public BeeDayHeroVariant Variant { get; set; } = BeeDayHeroVariant.Default;
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string CssClass
    {
        get
        {
            var variantClass = Variant switch
            {
                BeeDayHeroVariant.Onboarding => "levelup-hero--onboarding",
                _ => null
            };

            return string.Join(' ', new[] { "levelup-hero", variantClass, Class }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
