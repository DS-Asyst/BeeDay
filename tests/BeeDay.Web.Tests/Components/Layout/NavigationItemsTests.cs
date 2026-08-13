using BeeDay.Web.Components.Layout;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class NavigationItemsTests
{
    [Fact]
    public void SharedSourceContainsOnlyRealRoutesAndSecureLogout()
    {
        using var context = new BunitContext();
        var cut = context.Render<NavigationItems>();
        var links = cut.FindAll("a.navigation-item");
        Assert.Equal(["/home", "/daily", "/wallet", "/account", "/settings"], links.Select(link => link.GetAttribute("href")));
        var logout = cut.Find("form[method='post'][action='/auth/logout'] button[type='submit']");
        Assert.Equal("Log out of BeeDay", logout.GetAttribute("aria-label"));
    }

    [Fact]
    public void EveryRouteInvokesMobileCloseCallback()
    {
        using var context = new BunitContext();
        var count = 0;
        var cut = context.Render<NavigationItems>(parameters => parameters.Add(component => component.OnNavigate, () => count++));
        foreach (var link in cut.FindAll("a.navigation-item"))
        {
            link.Click();
        }
        Assert.Equal(5, count);
    }
}
