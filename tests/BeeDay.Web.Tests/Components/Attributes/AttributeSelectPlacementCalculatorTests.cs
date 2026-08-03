using LevelUp.Web.Components.DesignSystem.Attributes;

namespace LevelUp.Web.Tests.Components.Attributes;

/// <summary>
/// Exercises the pure placement geometry in isolation from any DOM/browser —
/// every scenario is expressed relative to the inputs (menu height, viewport
/// size, margin) rather than as hardcoded absolute pixel expectations, so
/// these stay meaningful if the numbers involved ever change.
/// </summary>
public sealed class AttributeSelectPlacementCalculatorTests
{
    private const double Margin = 8;
    private const double Gap = 6;

    private static AttributeSelectGeometry Geometry(
        double triggerTop, double triggerBottom, double triggerLeft = 40, double triggerWidth = 200,
        double menuHeight = 180, double viewportHeight = 800) =>
        new(triggerTop, triggerBottom, triggerLeft, triggerWidth, menuHeight, viewportHeight);

    [Fact]
    public void OpensBelowWhenSpaceBelowFitsTheMenu()
    {
        var geometry = Geometry(triggerTop: 100, triggerBottom: 130);

        var placement = AttributeSelectPlacementCalculator.Calculate(geometry, Margin, Gap);

        Assert.False(placement.FlipUp);
        Assert.Equal(geometry.TriggerBottom + Gap, placement.TopPx);
        Assert.Equal(geometry.MenuHeight, placement.MaxHeightPx);
    }

    [Fact]
    public void OpensAboveWhenSpaceBelowIsInsufficientButSpaceAboveIsNot()
    {
        const double viewportHeight = 800;
        const double menuHeight = 180;
        var triggerBottom = viewportHeight - Margin - Gap - (menuHeight / 2);
        var geometry = Geometry(triggerTop: triggerBottom - 30, triggerBottom: triggerBottom,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = AttributeSelectPlacementCalculator.Calculate(geometry, Margin, Gap);

        Assert.True(placement.FlipUp);
        Assert.Equal(geometry.TriggerTop - Gap - placement.MaxHeightPx, placement.TopPx);
    }

    [Fact]
    public void PicksTheSideWithMoreRoomWhenNeitherSideFullyFitsTheMenu()
    {
        const double viewportHeight = 400;
        const double menuHeight = 300;
        var triggerTop = (viewportHeight / 2) + 10;
        var triggerBottom = triggerTop + 20;
        var geometry = Geometry(triggerTop: triggerTop, triggerBottom: triggerBottom,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = AttributeSelectPlacementCalculator.Calculate(geometry, Margin, Gap);

        var spaceBelow = viewportHeight - triggerBottom - Gap - Margin;
        var spaceAbove = triggerTop - Gap - Margin;
        Assert.True(spaceAbove > spaceBelow, "test setup should make 'above' the roomier side");
        Assert.True(placement.FlipUp);
    }

    [Fact]
    public void ClampsMaxHeightToAvailableSpaceWhenTheMenuDoesNotFullyFit()
    {
        const double viewportHeight = 300;
        const double menuHeight = 250;
        var geometry = Geometry(triggerTop: 200, triggerBottom: 220,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = AttributeSelectPlacementCalculator.Calculate(geometry, Margin, Gap);

        var spaceAbove = geometry.TriggerTop - Gap - Margin;
        Assert.True(placement.FlipUp);
        Assert.Equal(spaceAbove, placement.MaxHeightPx);
        Assert.True(placement.MaxHeightPx < menuHeight);
    }

    [Fact]
    public void StaysBelowWhenNeitherSideFitsButBelowHasMoreRoom()
    {
        const double viewportHeight = 400;
        const double menuHeight = 300;
        var triggerTop = (viewportHeight / 2) - 30;
        var triggerBottom = triggerTop + 20;
        var geometry = Geometry(triggerTop: triggerTop, triggerBottom: triggerBottom,
            menuHeight: menuHeight, viewportHeight: viewportHeight);

        var placement = AttributeSelectPlacementCalculator.Calculate(geometry, Margin, Gap);

        var spaceBelow = viewportHeight - triggerBottom - Gap - Margin;
        var spaceAbove = triggerTop - Gap - Margin;
        Assert.True(spaceBelow > spaceAbove, "test setup should make 'below' the roomier side");
        Assert.False(placement.FlipUp);
    }
}
