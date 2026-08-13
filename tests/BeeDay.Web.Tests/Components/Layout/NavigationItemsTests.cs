using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class NavigationItemsTests
{
    [Fact]
    public void RendersOnlyRealDestinationsNoDeadLinksNoArtificialPages()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavigationItems>();

        var routeLinks = cut.FindAll("nav.navigation-items a");
        Assert.Equal(3, routeLinks.Count);
        Assert.Equal("/home", routeLinks[0].GetAttribute("href"));
        Assert.Equal("/daily", routeLinks[1].GetAttribute("href"));
        Assert.Equal("/wallet", routeLinks[2].GetAttribute("href"));
        Assert.All(routeLinks, link => Assert.NotEqual("#", link.GetAttribute("href")));

        var actionButtons = cut.FindAll(".navigation-items--actions button");
        Assert.Equal(2, actionButtons.Count);
    }

    [Fact]
    public void RouteItemClick_InvokesOnNavigate()
    {
        using var context = new BunitContext();
        var navigateCount = 0;

        var cut = context.Render<NavigationItems>(parameters => parameters
            .Add(component => component.OnNavigate, () => navigateCount++));

        cut.Find("a[href='/home']").Click();
        Assert.Equal(1, navigateCount);

        cut.Find("a[href='/daily']").Click();
        Assert.Equal(2, navigateCount);

        cut.Find("a[href='/wallet']").Click();
        Assert.Equal(3, navigateCount);
    }

    [Fact]
    public void ActionItemClicks_InvokeTheirOwnCallbackOnlyNotOnNavigate()
    {
        using var context = new BunitContext();
        var navigateCount = 0;
        var profileToggled = false;
        var accountToggled = false;

        var cut = context.Render<NavigationItems>(parameters => parameters
            .Add(component => component.OnNavigate, () => navigateCount++)
            .Add(component => component.OnToggleProfilePanel, () => profileToggled = true)
            .Add(component => component.OnToggleMenuPanel, () => accountToggled = true));

        cut.Find("button[aria-label='Open profile panel']").Click();
        Assert.True(profileToggled);
        Assert.False(accountToggled);
        Assert.Equal(0, navigateCount);

        cut.Find("button[aria-label='Open support menu']").Click();
        Assert.True(accountToggled);
        Assert.Equal(0, navigateCount);
    }
}
