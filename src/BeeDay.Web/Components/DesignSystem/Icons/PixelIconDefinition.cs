namespace BeeDay.Web.Components.DesignSystem.Icons;

public sealed record PixelIconDefinition(
    string SymbolId,
    string AssetPath,
    PixelIconCategory Category,
    string SemanticName,
    string? DefaultLabel = null,
    PixelIconName? Fallback = null);
