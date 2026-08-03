namespace LevelUp.Web.Components.DesignSystem.Attributes;

/// <summary>
/// Raw geometry measured from the real DOM (trigger, the menu panel itself,
/// and the viewport) — gathered by <c>activity-attribute-select.js</c>'s
/// <c>measureGeometry</c> after the panel has been rendered (invisible, but
/// laid out) so its actual height is known, never estimated.
/// </summary>
public readonly record struct AttributeSelectGeometry(
    double TriggerTop,
    double TriggerBottom,
    double TriggerLeft,
    double TriggerWidth,
    double MenuHeight,
    double ViewportHeight);

/// <summary>The computed placement: whether the menu flips above the trigger, its top offset, and its clamped height — all as ready-to-apply pixel values.</summary>
public readonly record struct AttributeSelectPlacement(bool FlipUp, double TopPx, double MaxHeightPx);

/// <summary>
/// Pure, viewport-aware placement geometry for <see cref="ActivityAttributeSelect"/>.
/// Deliberately framework-free (no Blazor/JS types) so it is testable with
/// plain xUnit — no browser, no bUnit render tree required.
/// </summary>
public static class AttributeSelectPlacementCalculator
{
    public static AttributeSelectPlacement Calculate(AttributeSelectGeometry geometry, double viewportMargin, double gap)
    {
        var spaceBelow = geometry.ViewportHeight - geometry.TriggerBottom - gap - viewportMargin;
        var spaceAbove = geometry.TriggerTop - gap - viewportMargin;

        // Below is the default. Flip up only when below doesn't fit AND
        // above actually offers more room — this also covers the "neither
        // side fits" case by picking whichever is less bad.
        var flipUp = spaceBelow < geometry.MenuHeight && spaceAbove > spaceBelow;

        var available = Math.Max(0, flipUp ? spaceAbove : spaceBelow);
        var maxHeight = Math.Min(geometry.MenuHeight, available);

        var top = flipUp
            ? geometry.TriggerTop - gap - maxHeight
            : geometry.TriggerBottom + gap;

        return new AttributeSelectPlacement(flipUp, top, maxHeight);
    }
}
