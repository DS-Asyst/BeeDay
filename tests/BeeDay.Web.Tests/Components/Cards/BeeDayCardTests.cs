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
            .Add(component => component.Prominent, true)
            .Add(component => component.Interactive, true));

        var article = cut.Find("article");
        Assert.Contains("beeday-card--padded", article.ClassList);
        Assert.Contains("beeday-card--muted", article.ClassList);
        Assert.Contains("beeday-card--prominent", article.ClassList);
        Assert.Contains("beeday-card--interactive", article.ClassList);
    }

    // EXP32-F020 (Sprint 32.13): role="status"/"alert" is not an ARIA-allowed role on <article>
    // (axe: aria-allowed-role) - a consumer requesting one of these live-region roles gets a plain
    // <div> host instead, since that is what the role actually requires.
    [Theory]
    [InlineData("status")]
    [InlineData("alert")]
    public void WhenRoleIsALiveRegion_RendersADivInsteadOfAnArticle(string role)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayCard>(parameters => parameters
            .AddUnmatched("role", role)
            .AddChildContent("Weekly activity unavailable"));

        Assert.Empty(cut.FindAll("article"));
        var div = cut.Find("div");
        Assert.Contains("beeday-card", div.ClassList);
        Assert.Equal(role, div.GetAttribute("role"));
        Assert.Equal("Weekly activity unavailable", div.TextContent);
    }

    [Fact]
    public void WhenRoleIsNotALiveRegion_StillRendersAnArticle()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayCard>(parameters => parameters
            .AddUnmatched("role", "button")
            .AddChildContent("Habit card"));

        Assert.NotEmpty(cut.FindAll("article"));
        Assert.Empty(cut.FindAll("div"));
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
