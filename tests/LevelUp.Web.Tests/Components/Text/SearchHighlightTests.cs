using LevelUp.Web.Components.DesignSystem.Text;

namespace LevelUp.Web.Tests.Components.Text;

public sealed class SearchHighlightTests
{
    [Fact]
    public void HighlightsEveryCaseInsensitiveOccurrence()
    {
        using var context = new BunitContext();
        var cut = context.Render<SearchHighlight>(parameters => parameters
            .Add(component => component.Text, "Study C# and study Blazor")
            .Add(component => component.SearchTerm, "study"));

        var matches = cut.FindAll("mark");
        Assert.Equal(2, matches.Count);
        Assert.Equal("Study", matches[0].TextContent);
        Assert.Equal("study", matches[1].TextContent);
    }

    [Fact]
    public void RendersPlainTextWhenSearchIsEmpty()
    {
        using var context = new BunitContext();
        var cut = context.Render<SearchHighlight>(parameters => parameters
            .Add(component => component.Text, "Read documentation"));

        Assert.Empty(cut.FindAll("mark"));
        Assert.Contains("Read documentation", cut.Markup);
    }

    [Fact]
    public void EncodesUserContentInsteadOfRenderingHtml()
    {
        using var context = new BunitContext();
        var cut = context.Render<SearchHighlight>(parameters => parameters
            .Add(component => component.Text, "<script>alert('x')</script>")
            .Add(component => component.SearchTerm, "alert"));

        Assert.Empty(cut.FindAll("script"));
        Assert.Single(cut.FindAll("mark"));
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }
}
