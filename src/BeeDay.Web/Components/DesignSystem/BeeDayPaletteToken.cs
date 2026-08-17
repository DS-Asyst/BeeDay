namespace BeeDay.Web.Components.DesignSystem;

/// <summary>
/// The ten-color beeday brand palette (COR0-COR9) — the only palette authorized for new hero
/// surfaces and Tag colors (EPIC 27, 03_DESIGN_DECISIONS.md §2). Centralizing the enum keeps
/// hero-surface and Tag-color consumers from diverging on which ten colors exist.
/// </summary>
public enum BeeDayPaletteToken
{
    Cor0,
    Cor1,
    Cor2,
    Cor3,
    Cor4,
    Cor5,
    Cor6,
    Cor7,
    Cor8,
    Cor9
}

public static class BeeDayPaletteTokenExtensions
{
    /// <summary>Maps the token to its <c>beeday-surface-corN</c> CSS class, defined in utilities.css.</summary>
    public static string ToSurfaceCssClass(this BeeDayPaletteToken token) => token switch
    {
        BeeDayPaletteToken.Cor0 => "beeday-surface-cor0",
        BeeDayPaletteToken.Cor1 => "beeday-surface-cor1",
        BeeDayPaletteToken.Cor2 => "beeday-surface-cor2",
        BeeDayPaletteToken.Cor3 => "beeday-surface-cor3",
        BeeDayPaletteToken.Cor4 => "beeday-surface-cor4",
        BeeDayPaletteToken.Cor5 => "beeday-surface-cor5",
        BeeDayPaletteToken.Cor6 => "beeday-surface-cor6",
        BeeDayPaletteToken.Cor7 => "beeday-surface-cor7",
        BeeDayPaletteToken.Cor8 => "beeday-surface-cor8",
        BeeDayPaletteToken.Cor9 => "beeday-surface-cor9",
        _ => "beeday-surface-cor0"
    };

    /// <summary>
    /// True for the two palette slots whose approved foreground is white (Cor0, Cor8); every other
    /// slot pairs with the dark Cor8 foreground (03_DESIGN_DECISIONS.md §2 contrast table). Consumers
    /// that layer a second element — such as an inverse brand lockup — on top of a chosen surface use
    /// this to pick the matching variant instead of guessing.
    /// </summary>
    public static bool IsWhiteForeground(this BeeDayPaletteToken token) =>
        token is BeeDayPaletteToken.Cor0 or BeeDayPaletteToken.Cor8;

    /// <summary>
    /// The token's literal hex value, matching wwwroot/css/variables.css's --beeday-palette-corN
    /// custom properties. Used where a real string value is required rather than a CSS class —
    /// e.g. the Wallet Tag color picker (EPIC 27 Sprint 27.11), which persists a hex string.
    /// </summary>
    public static string ToHexColor(this BeeDayPaletteToken token) => token switch
    {
        BeeDayPaletteToken.Cor0 => "#5247F9",
        BeeDayPaletteToken.Cor1 => "#CE82FF",
        BeeDayPaletteToken.Cor2 => "#58CC02",
        BeeDayPaletteToken.Cor3 => "#1CB0F6",
        BeeDayPaletteToken.Cor4 => "#FFB100",
        BeeDayPaletteToken.Cor5 => "#FF7878",
        BeeDayPaletteToken.Cor6 => "#FFFFFF",
        BeeDayPaletteToken.Cor7 => "#ECECED",
        BeeDayPaletteToken.Cor8 => "#100F3E",
        BeeDayPaletteToken.Cor9 => "#DEFFF7",
        _ => "#5247F9"
    };
}
