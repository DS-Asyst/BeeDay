using LevelUp.Web.Components.DesignSystem.Cards;

namespace LevelUp.Web.Tests.Components.Cards;

public sealed class LevelUpCardTests
{
    [Fact]
    public void RendersSemanticArticleAndContent()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpCard>(parameters => parameters
            .AddChildContent("Card content"));

        var article = cut.Find("article");
        Assert.Contains("levelup-card", article.ClassList);
        Assert.Equal("Card content", article.TextContent);
    }

    [Fact]
    public void AppendsCustomClassAndAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpCard>(parameters => parameters
            .Add(component => component.Class, "habit-card")
            .AddUnmatched("aria-label", "Habit"));

        var article = cut.Find("article");
        Assert.Contains("habit-card", article.ClassList);
        Assert.Equal("Habit", article.GetAttribute("aria-label"));
    }
}
