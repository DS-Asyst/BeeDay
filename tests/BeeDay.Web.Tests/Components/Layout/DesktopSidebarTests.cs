using BeeDay.Web.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class DesktopSidebarTests
{
    [Fact]
    public void RendersBrandLinkToDaily()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false));

        var brand = cut.Find("a.desktop-sidebar__brand-link");
        Assert.Equal("/daily", brand.GetAttribute("href"));
    }

    [Fact]
    public void RendersOnlyRealDestinationsNoDeadLinks()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false));

        var routeLinks = cut.FindAll("nav.navigation-items a.navigation-item");
        Assert.Equal(2, routeLinks.Count);
        Assert.Equal("/daily", routeLinks[0].GetAttribute("href"));
        Assert.Equal("/wallet", routeLinks[1].GetAttribute("href"));
        Assert.All(routeLinks, link => Assert.NotEqual("#", link.GetAttribute("href")));

        var actionButtons = cut.FindAll(".navigation-items--actions button.navigation-item");
        Assert.Equal(2, actionButtons.Count);
    }

    [Fact]
    public void ReflectsClosedPanelStateInAriaAttributesAndLabels()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false));

        var profileButton = cut.Find("button[aria-label='Open profile panel']");
        Assert.Equal("false", profileButton.GetAttribute("aria-expanded"));

        var accountButton = cut.Find("button[aria-label='Open support menu']");
        Assert.Equal("false", accountButton.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ReflectsOpenPanelStateInAriaAttributesAndLabels()
    {
        using var context = new BunitContext();

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, true)
            .Add(component => component.IsMenuPanelOpen, true));

        var profileButton = cut.Find("button[aria-label='Close profile panel']");
        Assert.Equal("true", profileButton.GetAttribute("aria-expanded"));
        Assert.Contains("is-active", profileButton.GetAttribute("class"));

        var accountButton = cut.Find("button[aria-label='Close support menu']");
        Assert.Equal("true", accountButton.GetAttribute("aria-expanded"));
        Assert.Contains("is-active", accountButton.GetAttribute("class"));
    }

    [Fact]
    public void InvokesTheCorrectCallbackPerTrigger()
    {
        using var context = new BunitContext();
        var profileToggled = false;
        var menuToggled = false;

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false)
            .Add(component => component.OnToggleProfilePanel, () => profileToggled = true)
            .Add(component => component.OnToggleMenuPanel, () => menuToggled = true));

        cut.Find("button[aria-label='Open profile panel']").Click();
        Assert.True(profileToggled);
        Assert.False(menuToggled);

        cut.Find("button[aria-label='Open support menu']").Click();
        Assert.True(menuToggled);
    }

    [Fact]
    public void MarksTheCurrentRouteActiveWithAriaCurrent()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/wallet");

        var cut = context.Render<DesktopSidebar>(parameters => parameters
            .Add(component => component.IsProfilePanelOpen, false)
            .Add(component => component.IsMenuPanelOpen, false));

        var dailyLink = cut.Find("a[href='/daily']");
        var walletLink = cut.Find("a[href='/wallet']");

        Assert.Null(dailyLink.GetAttribute("aria-current"));
        Assert.Equal("page", walletLink.GetAttribute("aria-current"));
    }
}
