using BeeDay.Web.Components.DesignSystem.Text;

namespace BeeDay.Web.Tests.Components.Text;

public sealed class LevelUpBrandTests
{
    [Fact]
    public void RendersAccessibleSharedBrand()
    {
        using var context = new BunitContext();

        var cut = context.Render<LevelUpBrand>();
        var brand = cut.Find(".levelup-brand");

        Assert.Equal("Level Up", brand.GetAttribute("aria-label"));
        Assert.Equal("LEVELUP", brand.TextContent.Trim());
        Assert.Contains("UP", cut.Find(".levelup-brand__accent").TextContent);
    }
}
