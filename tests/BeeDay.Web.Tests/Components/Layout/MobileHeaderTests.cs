using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class MobileHeaderTests
{
    [Fact]
    public void RendersBrandLinkToDaily()
    {
        using var context = new BunitContext();

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false));

        var brand = cut.Find("a.mobile-header__brand");
        Assert.Equal("/home", brand.GetAttribute("href"));
    }

    [Fact]
    public void ClosedState_HasCorrectAriaContract()
    {
        using var context = new BunitContext();

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false));

        var button = cut.Find(".mobile-header__menu-button");
        Assert.Equal("Open navigation menu", button.GetAttribute("aria-label"));
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
        Assert.Equal("mobile-navigation", button.GetAttribute("aria-controls"));
    }

    [Fact]
    public void OpenState_HasCorrectAriaContract()
    {
        using var context = new BunitContext();

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, true));

        var button = cut.Find(".mobile-header__menu-button");
        Assert.Equal("Close navigation menu", button.GetAttribute("aria-label"));
        Assert.Equal("true", button.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void MenuButtonClick_InvokesOnToggleNav()
    {
        using var context = new BunitContext();
        var toggled = false;

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false)
            .Add(component => component.OnToggleNav, () => toggled = true));

        cut.Find(".mobile-header__menu-button").Click();
        Assert.True(toggled);
    }
}
