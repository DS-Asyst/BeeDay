using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class DesktopSidebarTests
{
    [Fact]
    public void RendersTheSameProfileAndMenuTriggersTopNavigationExposedTodayReusedNotReinvented()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false));

        var brandButton = cut.Find(".desktop-sidebar__brand-button");
        Assert.Equal("Open profile panel", brandButton.GetAttribute("aria-label"));
        Assert.False(brandButton.HasAttribute("aria-expanded"));

        var menuButton = cut.Find(".desktop-sidebar__menu-button");
        Assert.Equal("Open support menu", menuButton.GetAttribute("aria-label"));
        Assert.False(menuButton.HasAttribute("aria-expanded"));

        var links = cut.FindAll(".desktop-sidebar__links a");
        Assert.Equal(2, links.Count);
        Assert.Equal("/daily", links[0].GetAttribute("href"));
        Assert.Equal("/wallet", links[1].GetAttribute("href"));
    }

    [Fact]
    public void ReflectsOpenPanelStateInAriaAttributesAndLabels()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, true)
            .Add(component => component.IsMenuPanelOpen, true));

        var brandButton = cut.Find(".desktop-sidebar__brand-button");
        Assert.Equal("Close profile panel", brandButton.GetAttribute("aria-label"));
        Assert.True(brandButton.HasAttribute("aria-expanded"));

        var menuButton = cut.Find(".desktop-sidebar__menu-button");
        Assert.Equal("Close support menu", menuButton.GetAttribute("aria-label"));
        Assert.True(menuButton.HasAttribute("aria-expanded"));
    }

    [Fact]
    public void InvokesTheSameCallbacksMainLayoutWiresTopNavigationToToday()
    {
        using var context = new BunitContext();
        var profileToggled = false;
        var menuToggled = false;

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false)
            .Add(component => component.OnToggleProfilePanel, () => profileToggled = true)
            .Add(component => component.OnToggleMenuPanel, () => menuToggled = true));

        cut.Find(".desktop-sidebar__brand-button").Click();
        Assert.True(profileToggled);

        cut.Find(".desktop-sidebar__menu-button").Click();
        Assert.True(menuToggled);
    }
}
