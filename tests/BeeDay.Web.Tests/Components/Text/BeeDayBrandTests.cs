using BeeDay.Web.Components.DesignSystem.Text;

namespace BeeDay.Web.Tests.Components.Text;

public sealed class BeeDayBrandTests
{
    [Fact]
    public void RendersAccessibleSharedBrand()
    {
        using var context = new BunitContext();

        var cut = context.Render<BeeDayBrand>();
        var brand = cut.Find(".beeday-brand");

        var image = cut.Find("img.beeday-brand__wordmark");

        Assert.Equal("/beeday-wordmark.png", image.GetAttribute("src"));
        Assert.Equal("BeeDay", image.GetAttribute("alt"));
        Assert.Equal("904", image.GetAttribute("width"));
        Assert.Equal("276", image.GetAttribute("height"));
        Assert.Null(brand.GetAttribute("aria-label"));
    }

    [Fact]
    public void AppliesContrastSurfaceOnlyWhenRequested()
    {
        using var context = new BunitContext();

        var defaultBrand = context.Render<BeeDayBrand>();
        var contrastBrand = context.Render<BeeDayBrand>(parameters => parameters
            .Add(component => component.OnDarkSurface, true));

        Assert.DoesNotContain("beeday-brand--contrast", defaultBrand.Find(".beeday-brand").ClassList);
        Assert.Contains("beeday-brand--contrast", contrastBrand.Find(".beeday-brand").ClassList);
    }
}
