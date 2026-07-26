using LevelUp.Web.Components.DesignSystem.Icons;

namespace LevelUp.Web.Tests.Components.Icons;

public sealed class PixelIconTests
{
    [Fact]
    public void RendersDecorativeIconFromSprite()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Search));

        var svg = cut.Find("svg");
        Assert.Equal("true", svg.GetAttribute("aria-hidden"));
        Assert.Null(svg.GetAttribute("role"));
        Assert.Equal("/icons/pixel/sprite.svg#search", cut.Find("use").GetAttribute("href"));
    }

    [Fact]
    public void RendersAccessibleLabelWhenNotDecorative()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Warning)
            .Add(component => component.Decorative, false)
            .Add(component => component.Label, "Warning status"));

        var svg = cut.Find("svg");
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Warning status", svg.GetAttribute("aria-label"));
        Assert.Null(svg.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void RejectsInformativeIconWithoutLabel()
    {
        using var context = new BunitContext();

        Assert.Throws<InvalidOperationException>(() => context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Information)
            .Add(component => component.Decorative, false)));
    }

    [Theory]
    [InlineData(PixelIconSize.ExtraSmall, "12")]
    [InlineData(PixelIconSize.Small, "16")]
    [InlineData(PixelIconSize.Medium, "20")]
    [InlineData(PixelIconSize.Large, "24")]
    [InlineData(PixelIconSize.ExtraLarge, "32")]
    public void MapsOfficialSizeTokens(PixelIconSize size, string pixels)
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Inventory)
            .Add(component => component.Size, size));

        var svg = cut.Find("svg");
        Assert.Equal(pixels, svg.GetAttribute("width"));
        Assert.Equal(pixels, svg.GetAttribute("height"));
        Assert.Contains($"pixel-icon--size-{size.ToString().ToLowerInvariant()}", svg.ClassList);
    }

    [Fact]
    public void AppliesColorAndCustomClasses()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, PixelIconName.Inventory)
            .Add(component => component.Color, PixelIconColor.Primary)
            .Add(component => component.Class, "menu-icon"));

        var svg = cut.Find("svg");
        Assert.Contains("pixel-icon--inventory", svg.ClassList);
        Assert.Contains("pixel-icon--color-primary", svg.ClassList);
        Assert.Contains("menu-icon", svg.ClassList);
    }

    [Fact]
    public void FallsBackWithoutBreakingRenderingForUnknownName()
    {
        using var context = new BunitContext();
        var cut = context.Render<PixelIcon>(parameters => parameters
            .Add(component => component.Name, (PixelIconName)999));

        Assert.Equal("Warning", cut.Find("svg").GetAttribute("data-icon"));
        Assert.Equal("/icons/pixel/sprite.svg#warning", cut.Find("use").GetAttribute("href"));
    }
}
