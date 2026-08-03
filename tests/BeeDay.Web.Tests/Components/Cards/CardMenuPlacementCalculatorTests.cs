using BeeDay.Web.Components.DesignSystem.Cards;

namespace BeeDay.Web.Tests.Components.Cards;

/// <summary>
/// Exercises the pure placement geometry in isolation from any DOM/browser —
/// every scenario is expressed relative to the inputs (menu height, viewport
/// size, margin) rather than as hardcoded absolute pixel expectations, so
/// these stay meaningful if the numbers involved ever change.
/// </summary>
public sealed class CardMenuPlacementCalculatorTests
{
    private const double Margin = 8;

    private static CardMenuGeometry Geometry(
        double triggerTop, double triggerBottom, double triggerLeft, double triggerRight,
        double menuWidth = 140, double menuHeight = 80,
        double viewportWidth = 1000, double viewportHeight = 800) =>
        new(triggerTop, triggerBottom, triggerLeft, triggerRight, menuWidth, menuHeight, viewportWidth, viewportHeight);

    [Fact]
    public void OpensBelowWhenSpaceBelowFitsTheMenu()
    {
        const double menuHeight = 80;
        const double viewportHeight = 800;
        // Trigger sits well above the point where "space below" would be tight.
        var geometry = Geometry(triggerTop: 100, triggerBottom: 120, triggerLeft: 50, triggerRight: 100,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        Assert.False(placement.FlipUp);
    }

    [Fact]
    public void OpensAboveWhenSpaceBelowIsInsufficientButSpaceAboveIsNot()
    {
        const double menuHeight = 80;
        const double viewportHeight = 800;
        // Trigger's bottom sits close enough to the viewport bottom that
        // (viewportHeight - triggerBottom - margin) < menuHeight, while the
        // trigger's top leaves ample room above.
        var triggerBottom = viewportHeight - Margin - (menuHeight / 2);
        var geometry = Geometry(triggerTop: triggerBottom - 20, triggerBottom: triggerBottom, triggerLeft: 50, triggerRight: 100,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        Assert.True(placement.FlipUp);
    }

    [Fact]
    public void PicksTheSideWithMoreRoomWhenNeitherSideFullyFitsTheMenu()
    {
        const double menuHeight = 300;
        const double viewportHeight = 400;
        // A short viewport where the menu doesn't fully fit on either side:
        // trigger roughly centered, but slightly closer to the bottom, so
        // "above" has strictly more room than "below".
        var triggerTop = (viewportHeight / 2) + 10;
        var triggerBottom = triggerTop + 20;
        var geometry = Geometry(triggerTop: triggerTop, triggerBottom: triggerBottom, triggerLeft: 50, triggerRight: 100,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        var spaceBelow = viewportHeight - triggerBottom - Margin;
        var spaceAbove = triggerTop - Margin;
        Assert.True(spaceAbove > spaceBelow, "test setup should make 'above' the roomier side");
        Assert.True(placement.FlipUp);
    }

    [Fact]
    public void StaysBelowWhenNeitherSideFitsButBelowHasMoreRoom()
    {
        const double menuHeight = 300;
        const double viewportHeight = 400;
        // Mirror of the previous case: trigger slightly closer to the top,
        // so "below" has strictly more room than "above".
        var triggerTop = (viewportHeight / 2) - 30;
        var triggerBottom = triggerTop + 20;
        var geometry = Geometry(triggerTop: triggerTop, triggerBottom: triggerBottom, triggerLeft: 50, triggerRight: 100,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        var spaceBelow = viewportHeight - triggerBottom - Margin;
        var spaceAbove = triggerTop - Margin;
        Assert.True(spaceBelow > spaceAbove, "test setup should make 'below' the roomier side");
        Assert.False(placement.FlipUp);
    }

    [Fact]
    public void NoHorizontalShiftWhenTheDefaultRightAlignedPositionAlreadyFitsInTheViewport()
    {
        const double menuWidth = 140;
        const double viewportWidth = 1000;
        // Trigger comfortably inside the viewport horizontally.
        var geometry = Geometry(triggerTop: 100, triggerBottom: 120, triggerLeft: 400, triggerRight: 460,
            menuWidth: menuWidth, viewportWidth: viewportWidth);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        Assert.Equal(0, placement.HorizontalShiftPx);
    }

    [Fact]
    public void ShiftsRightWhenTheDefaultPositionWouldOverflowTheLeftViewportEdge()
    {
        const double menuWidth = 140;
        // Trigger near the left edge: right-aligning the menu under it
        // (triggerRight - menuWidth) would push the menu's left edge past 0.
        var geometry = Geometry(triggerTop: 100, triggerBottom: 120, triggerLeft: 5, triggerRight: 40,
            menuWidth: menuWidth);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        var desiredLeft = geometry.TriggerRight - menuWidth;
        Assert.True(desiredLeft < Margin, "test setup should make the default position overflow the left edge");
        Assert.True(placement.HorizontalShiftPx > 0);

        var finalLeft = desiredLeft + placement.HorizontalShiftPx;
        Assert.Equal(Margin, finalLeft, precision: 6);
    }

    [Fact]
    public void ShiftsLeftWhenTheDefaultPositionWouldOverflowTheRightViewportEdge()
    {
        const double menuWidth = 140;
        const double viewportWidth = 1000;
        // Trigger near the right edge: right-aligning the menu under it
        // would push the menu's right edge past the viewport width.
        var geometry = Geometry(triggerTop: 100, triggerBottom: 120, triggerLeft: 980, triggerRight: 1020,
            menuWidth: menuWidth, viewportWidth: viewportWidth);

        var placement = CardMenuPlacementCalculator.Calculate(geometry, Margin);

        var desiredLeft = geometry.TriggerRight - menuWidth;
        var upperBound = viewportWidth - menuWidth - Margin;
        Assert.True(desiredLeft > upperBound, "test setup should make the default position overflow the right edge");
        Assert.True(placement.HorizontalShiftPx < 0);

        var finalLeft = desiredLeft + placement.HorizontalShiftPx;
        Assert.Equal(upperBound, finalLeft, precision: 6);
    }
}
