namespace BeeDay.Web.Components.DesignSystem.Cards;

/// <summary>
/// Raw geometry measured from the real DOM (trigger, the menu panel itself,
/// and the viewport) — gathered by <c>beeday-card-menu.js</c>'s
/// <c>measureGeometry</c> after the panel has been rendered (invisible, but
/// laid out) so its actual width/height are known, never estimated.
/// </summary>
public readonly record struct CardMenuGeometry(
    double TriggerTop,
    double TriggerBottom,
    double TriggerLeft,
    double TriggerRight,
    double MenuWidth,
    double MenuHeight,
    double ViewportWidth,
    double ViewportHeight);

/// <summary>The computed placement: vertical flip and a horizontal nudge applied as a CSS transform.</summary>
public readonly record struct CardMenuPlacement(bool FlipUp, double HorizontalShiftPx);

/// <summary>
/// Pure, viewport-aware placement geometry for <see cref="BeeDayCardMenu"/>.
/// Deliberately framework-free (no Blazor/JS types) so it is testable with
/// plain xUnit — no browser, no bUnit render tree required.
/// </summary>
public static class CardMenuPlacementCalculator
{
    public static CardMenuPlacement Calculate(CardMenuGeometry geometry, double viewportMargin)
    {
        var spaceBelow = geometry.ViewportHeight - geometry.TriggerBottom - viewportMargin;
        var spaceAbove = geometry.TriggerTop - viewportMargin;

        // Below is the default. Flip up only when below doesn't fit AND
        // above actually offers more room — this also covers the "neither
        // side fits" case by picking whichever is less bad.
        var flipUp = spaceBelow < geometry.MenuHeight && spaceAbove > spaceBelow;

        // Horizontal: the panel's own CSS aligns its right edge with the
        // trigger's right edge by default (right: 0 relative to the
        // trigger's positioned ancestor). Clamp that desired position inside
        // the viewport (with `viewportMargin` on both sides) and express any
        // correction as a translateX shift so the CSS anchor is untouched
        // when no clamping is needed.
        var desiredLeft = geometry.TriggerRight - geometry.MenuWidth;
        var lowerBound = viewportMargin;
        var upperBound = Math.Max(viewportMargin, geometry.ViewportWidth - geometry.MenuWidth - viewportMargin);
        var clampedLeft = Math.Clamp(desiredLeft, lowerBound, upperBound);

        return new CardMenuPlacement(flipUp, clampedLeft - desiredLeft);
    }
}
