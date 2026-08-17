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
        Assert.Equal("beeday", brand.GetAttribute("aria-label"));
        Assert.Equal("bee", brand.QuerySelector(".beeday-brand__bee")?.TextContent);
        Assert.Equal("day", brand.QuerySelector(".beeday-brand__day")?.TextContent);
    }

    [Fact]
    public void RendersTheApprovedBeeIconByDefaultAndCanOptOut()
    {
        using var context = new BunitContext();
        var withIcon = context.Render<BeeDayBrand>().Find(".beeday-brand");
        var iconImage = withIcon.QuerySelector(".beeday-brand__icon");
        Assert.NotNull(iconImage);
        Assert.Equal("/assets/brand/bee.png", iconImage.GetAttribute("src"));
        Assert.Equal("", iconImage.GetAttribute("alt"));
        Assert.Equal("true", iconImage.GetAttribute("aria-hidden"));

        var withoutIcon = context.Render<BeeDayBrand>(parameters => parameters.Add(x => x.ShowIcon, false))
            .Find(".beeday-brand");
        Assert.Null(withoutIcon.QuerySelector(".beeday-brand__icon"));
    }

    [Fact]
    public void MergesCustomClassAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayBrand>(parameters => parameters
            .Add(x => x.Class, "public-header__brand-mark")
            .AddUnmatched("data-testid", "header-brand"));

        var brand = cut.Find(".beeday-brand");
        Assert.Contains("public-header__brand-mark", brand.ClassList);
        Assert.Equal("header-brand", brand.GetAttribute("data-testid"));
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

    [Fact]
    public void OnDarkSurface_SwapsTheIconToTheApprovedNonWhiteBackgroundVariant()
    {
        using var context = new BunitContext();

        var defaultIcon = context.Render<BeeDayBrand>().Find(".beeday-brand__icon");
        Assert.Equal("/assets/brand/bee.png", defaultIcon.GetAttribute("src"));
        Assert.Equal("279", defaultIcon.GetAttribute("width"));
        Assert.Equal("287", defaultIcon.GetAttribute("height"));

        var onDarkSurfaceIcon = context.Render<BeeDayBrand>(parameters => parameters.Add(x => x.OnDarkSurface, true))
            .Find(".beeday-brand__icon");
        Assert.Equal("/assets/brand/bee-color-neutral.png", onDarkSurfaceIcon.GetAttribute("src"));
        Assert.Equal("1254", onDarkSurfaceIcon.GetAttribute("width"));
        Assert.Equal("1254", onDarkSurfaceIcon.GetAttribute("height"));
    }
}
