using BeeDay.Web.Components.DesignSystem;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class BeeDayPaletteTokenTests
{
    [Theory]
    [InlineData(BeeDayPaletteToken.Cor0, "#5247F9")]
    [InlineData(BeeDayPaletteToken.Cor1, "#CE82FF")]
    [InlineData(BeeDayPaletteToken.Cor2, "#58CC02")]
    [InlineData(BeeDayPaletteToken.Cor3, "#1CB0F6")]
    [InlineData(BeeDayPaletteToken.Cor4, "#FFB100")]
    [InlineData(BeeDayPaletteToken.Cor5, "#FF7878")]
    [InlineData(BeeDayPaletteToken.Cor6, "#FFFFFF")]
    [InlineData(BeeDayPaletteToken.Cor7, "#ECECED")]
    [InlineData(BeeDayPaletteToken.Cor8, "#100F3E")]
    [InlineData(BeeDayPaletteToken.Cor9, "#DEFFF7")]
    public void ToHexColor_MatchesTheCssVariableItMirrors(BeeDayPaletteToken token, string expectedHex)
    {
        // EPIC 27 Sprint 27.11: the Wallet Tag color picker needs a real hex string (persisted
        // data), not just a CSS class — this keeps that single source of truth aligned with
        // wwwroot/css/variables.css's --beeday-palette-corN custom properties.
        Assert.Equal(expectedHex, token.ToHexColor(), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void EveryPaletteTokenHasAUniqueHexValue()
    {
        var hexValues = Enum.GetValues<BeeDayPaletteToken>().Select(token => token.ToHexColor()).ToArray();
        Assert.Equal(hexValues.Length, hexValues.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
