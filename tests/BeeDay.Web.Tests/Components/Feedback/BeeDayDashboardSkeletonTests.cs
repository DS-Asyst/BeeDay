using BeeDay.Web.Components.DesignSystem.Feedback;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class BeeDayDashboardSkeletonTests
{
    [Fact]
    public void WithoutAnExplicitAriaLabel_KeepsTheOriginalUnlocalizedLiteral()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayDashboardSkeleton>();

        Assert.Equal("Loading dashboard", cut.Find("section.dashboard-skeleton").GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData("Loading dashboard")]
    [InlineData("Carregando painel")]
    public void AnExplicitAriaLabel_OverridesTheDefault(string ariaLabel)
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayDashboardSkeleton>(parameters => parameters
            .Add(component => component.AriaLabel, ariaLabel));

        Assert.Equal(ariaLabel, cut.Find("section.dashboard-skeleton").GetAttribute("aria-label"));
    }
}
