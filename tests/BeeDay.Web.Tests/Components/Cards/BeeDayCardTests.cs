using BeeDay.Web.Components.DesignSystem.Cards;

namespace BeeDay.Web.Tests.Components.Cards;

public sealed class BeeDayCardTests
{
    [Fact]
    public void RendersSemanticArticleAndContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayCard>(parameters => parameters
            .AddChildContent("Card content"));

        var article = cut.Find("article");
        Assert.Contains("beeday-card", article.ClassList);
        Assert.Equal("Card content", article.TextContent);
    }

    [Fact]
    public void AppliesReusableSurfaceModifiers()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayCard>(parameters => parameters
            .Add(component => component.Padded, true)
            .Add(component => component.Muted, true)
            .Add(component => component.Interactive, true));

        var article = cut.Find("article");
        Assert.Contains("beeday-card--padded", article.ClassList);
        Assert.Contains("beeday-card--muted", article.ClassList);
        Assert.Contains("beeday-card--interactive", article.ClassList);
    }

    [Fact]
    public void AppendsCustomClassAndAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayCard>(parameters => parameters
            .Add(component => component.Class, "habit-card")
            .AddUnmatched("aria-label", "Habit"));

        var article = cut.Find("article");
        Assert.Contains("habit-card", article.ClassList);
        Assert.Equal("Habit", article.GetAttribute("aria-label"));
    }
}
