using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class MobileSidebarTests
{
    [Fact]
    public void ClosedState_IsAriaHiddenAndCarriesTheIdMobileHeaderPointsAt()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, false));

        var drawer = cut.Find("aside#mobile-navigation");
        Assert.Equal("true", drawer.GetAttribute("aria-hidden"));
        Assert.DoesNotContain("is-open", drawer.GetAttribute("class"));
    }

    [Fact]
    public void OpenState_IsNotAriaHiddenAndCarriesTheOpenClass()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true));

        var drawer = cut.Find("aside#mobile-navigation");
        Assert.Equal("false", drawer.GetAttribute("aria-hidden"));
        Assert.Contains("is-open", drawer.GetAttribute("class"));
    }

    [Fact]
    public void CloseButtonClick_InvokesOnClose()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var closed = false;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.OnClose, () => closed = true));

        cut.Find(".mobile-nav-drawer__close").Click();
        Assert.True(closed);
    }

    [Fact]
    public void BackdropClick_InvokesOnClose()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var closed = false;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.OnClose, () => closed = true));

        cut.Find(".mobile-nav-backdrop").Click();
        Assert.True(closed);
    }

    [Fact]
    public void EscapeKey_InvokesOnClose()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var closed = false;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.OnClose, () => closed = true));

        cut.Find("aside#mobile-navigation").KeyDown("Escape");
        Assert.True(closed);
    }

    [Fact]
    public void ContainsTheSameRealDestinationsAsDesktop()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true));

        var routeLinks = cut.FindAll("nav.navigation-items a");
        Assert.Equal(3, routeLinks.Count);
        Assert.Equal("/home", routeLinks[0].GetAttribute("href"));
        Assert.Equal("/daily", routeLinks[1].GetAttribute("href"));
        Assert.Equal("/wallet", routeLinks[2].GetAttribute("href"));
    }

    [Fact]
    public void NavigatingViaARouteItem_AlsoClosesTheDrawer()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var closed = false;

        var cut = context.Render<MobileSidebar>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.OnClose, () => closed = true));

        cut.Find("a[href='/daily']").Click();
        Assert.True(closed);
    }

    [Fact]
    public void ContainsProfileAccountAndLogoutWithoutLegacyPanelTriggers()
    {
        using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        var cut = context.Render<MobileSidebar>(parameters => parameters.Add(component => component.IsOpen, true));
        Assert.NotNull(cut.Find("a[href='/account']"));
        Assert.NotNull(cut.Find("a[href='/settings']"));
        Assert.NotNull(cut.Find("form[action='/auth/logout'] button[type='submit']"));
        Assert.Empty(cut.FindAll("button[aria-expanded]"));
    }
}
