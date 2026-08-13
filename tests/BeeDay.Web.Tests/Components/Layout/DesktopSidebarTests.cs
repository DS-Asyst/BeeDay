using BeeDay.Web.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class DesktopSidebarTests
{
    [Fact]
    public void ComposesOfficialBrandPrimaryRoutesAndSecondaryAccountActions()
    {
        using var context = new BunitContext();
        var cut = context.Render<DesktopSidebar>();

        Assert.Equal("/home", cut.Find("a.desktop-sidebar__brand-link").GetAttribute("href"));
        var primary = cut.FindAll("nav.navigation-items a.navigation-item");
        Assert.Equal(["/home", "/daily", "/wallet"], primary.Select(link => link.GetAttribute("href")));
        Assert.NotNull(cut.Find(".navigation-items--actions a[href='/account']"));
        Assert.NotNull(cut.Find(".navigation-items--actions a[href='/settings']"));
        Assert.Equal("/auth/logout", cut.Find("form.navigation-items__logout-form").GetAttribute("action"));
    }

    [Fact]
    public void MarksCurrentRouteWithAriaCurrent()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/wallet");
        var cut = context.Render<DesktopSidebar>();
        Assert.Equal("page", cut.Find("a[href='/wallet']").GetAttribute("aria-current"));
        Assert.Null(cut.Find("a[href='/daily']").GetAttribute("aria-current"));
    }
}
