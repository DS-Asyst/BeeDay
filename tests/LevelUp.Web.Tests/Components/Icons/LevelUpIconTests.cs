using LevelUp.Web.Components.DesignSystem.Icons;

namespace LevelUp.Web.Tests.Components.Icons;

public sealed class LevelUpIconTests
{
    [Fact]
    public void RendersAccessibleLabelWhenNotDecorative()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpIcon>(parameters => parameters
            .Add(component => component.Name, LevelUpIconName.Warning)
            .Add(component => component.Decorative, false)
            .Add(component => component.Label, "Warning"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Warning", cut.Find("title").TextContent);
        Assert.Contains("warning.svg", cut.Find("image").GetAttribute("href"));
    }

    [Fact]
    public void UsesRequestedPixelSizeAndClass()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpIcon>(parameters => parameters
            .Add(component => component.Name, LevelUpIconName.Inventory)
            .Add(component => component.Size, 24)
            .Add(component => component.Class, "menu-icon"));

        var svg = cut.Find("svg");
        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("24", svg.GetAttribute("height"));
        Assert.Contains("levelup-icon--inventory", svg.ClassList);
        Assert.Contains("menu-icon", svg.ClassList);
    }
}
