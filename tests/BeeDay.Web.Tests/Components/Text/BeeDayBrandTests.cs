using BeeDay.Web.Components.DesignSystem.Text;

namespace BeeDay.Web.Tests.Components.Text;

public sealed class BeeDayBrandTests
{
    [Fact]
    public void RendersAccessibleSharedBrand()
    {
        using var context = new BunitContext();
        var brand = context.Render<BeeDayBrand>().Find(".beeday-brand");
        Assert.Equal("img", brand.GetAttribute("role"));
        Assert.Equal("BeeDay", brand.GetAttribute("aria-label"));
        Assert.Equal("bee", brand.QuerySelector(".beeday-brand__bee")?.TextContent);
        Assert.Equal("day", brand.QuerySelector(".beeday-brand__day")?.TextContent);
    }

    [Fact]
    public void AppliesInverseVariantOnlyWhenRequested()
    {
        using var context = new BunitContext();
        var defaultBrand = context.Render<BeeDayBrand>();
        var inverseBrand = context.Render<BeeDayBrand>(parameters => parameters.Add(x => x.OnDarkSurface, true));
        Assert.DoesNotContain("beeday-brand--inverse", defaultBrand.Find(".beeday-brand").ClassList);
        Assert.Contains("beeday-brand--inverse", inverseBrand.Find(".beeday-brand").ClassList);
    }
}
