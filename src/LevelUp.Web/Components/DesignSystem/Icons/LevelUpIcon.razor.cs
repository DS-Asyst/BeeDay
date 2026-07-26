using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.DesignSystem.Icons;

public partial class LevelUpIcon
{
    [Parameter, EditorRequired] public LevelUpIconName Name { get; set; }
    [Parameter] public int Size { get; set; } = 20;
    [Parameter] public bool Decorative { get; set; } = true;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Class { get; set; }

    private string CssClasses => string.Join(' ', new[]
    {
        "levelup-icon",
        $"levelup-icon--{Name.ToString().ToLowerInvariant()}",
        Class
    }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private string IconPath => $"/icons/pixel/{GetFileName(Name)}.svg";

    private static string GetFileName(LevelUpIconName name) => name switch
    {
        LevelUpIconName.ChevronDown => "chevron-down",
        LevelUpIconName.ChevronLeft => "chevron-left",
        LevelUpIconName.ChevronRight => "chevron-right",
        _ => name.ToString().ToLowerInvariant()
    };
}
