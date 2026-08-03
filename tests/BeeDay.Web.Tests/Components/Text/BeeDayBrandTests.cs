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

        Assert.Equal("Bee Day", brand.GetAttribute("aria-label"));
        Assert.Equal("BEEDAY", brand.TextContent.Trim());
        Assert.Contains("DAY", cut.Find(".beeday-brand__accent").TextContent);
    }
}
