using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard.Components;

namespace BeeDay.Web.Tests.Components.Dashboard;

public sealed class HabitCardMetricsTests
{
    [Theory]
    [InlineData(42, 0, "+42")]
    [InlineData(0, 0, "0")]
    [InlineData(0, 20, "-20")]
    [InlineData(0, 9999, "-9999")]
    [InlineData(12345, 0, "+12345")]
    public void RendersTheBalanceCounterWithAnExplicitSignAndNoLayoutShiftRegardlessOfDigitCount(int positiveCount, int negativeCount, string expectedText)
    {
        using var context = new BunitContext();
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both)
            .Add(component => component.PositiveCount, positiveCount)
            .Add(component => component.NegativeCount, negativeCount));

        var balance = cut.Find(".habit-card__balance");

        Assert.Equal(expectedText, balance.TextContent);
    }

    [Fact]
    public void RendersTheMetricsColumnWithTheStatisticsIconBesideTheCounter()
    {
        using var context = new BunitContext();
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both));

        var metrics = cut.Find(".habit-card__metrics");
        var children = metrics.Children;

        Assert.Equal(2, children.Length);
        Assert.Equal("svg", children[0].TagName, ignoreCase: true);
        Assert.Contains("habit-card__balance", children[1].ClassList);
    }

    [Fact]
    public void DoesNotRenderARepeatIconInTheMetricsColumn()
    {
        using var context = new BunitContext();
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both));

        Assert.DoesNotContain("pixel-icon--repeat", cut.Find(".habit-card__metrics").InnerHtml, StringComparison.Ordinal);
    }

    [Fact]
    public void ExposesTheHabitDirectionAsAnAccessibleLabelOnTheMetricsColumn()
    {
        using var context = new BunitContext();
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Negative));

        Assert.Equal("Negative habit", cut.Find(".habit-card__metrics").GetAttribute("aria-label"));
    }
}
