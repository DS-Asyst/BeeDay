using BeeDay.Web.Components.DesignSystem.Icons;
using BeeDay.Web.Components.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class NavigationItemTests
{
    [Fact]
    public void RouteMode_RendersALinkWithIconAndLabel()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Daily)
            .Add(component => component.Label, "Daily")
            .Add(component => component.Href, "/daily"));

        var link = cut.Find("a.navigation-item");
        Assert.Equal("/daily", link.GetAttribute("href"));
        Assert.Contains("Daily", link.TextContent);
        Assert.Empty(cut.FindAll("button"));
    }

    [Fact]
    public void ActionMode_RendersAButtonNotALink()
    {
        using var context = new BunitContext();
        var clicked = false;

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Profile)
            .Add(component => component.Label, "Profile")
            .Add(component => component.AriaLabel, "Open profile panel")
            .Add(component => component.OnClick, () => clicked = true));

        var button = cut.Find("button.navigation-item");
        Assert.Equal("Open profile panel", button.GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("a"));

        button.Click();
        Assert.True(clicked);
    }

    [Fact]
    public void ActionMode_RendersLiteralAriaExpandedStringsNotBooleanAttributePresenceAbsence()
    {
        using var context = new BunitContext();

        var closed = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Profile)
            .Add(component => component.Label, "Profile")
            .Add(component => component.AriaExpanded, false));
        Assert.Equal("false", closed.Find("button").GetAttribute("aria-expanded"));

        var open = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Profile)
            .Add(component => component.Label, "Profile")
            .Add(component => component.AriaExpanded, true));
        Assert.Equal("true", open.Find("button").GetAttribute("aria-expanded"));
        Assert.Contains("is-active", open.Find("button").GetAttribute("class"));
    }

    [Fact]
    public void ActionMode_OmitsAriaExpandedWhenNotProvided()
    {
        using var context = new BunitContext();

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Profile)
            .Add(component => component.Label, "Profile"));

        Assert.False(cut.Find("button").HasAttribute("aria-expanded"));
    }

    [Fact]
    public void RouteMode_SetsAriaCurrentPageOnlyWhenTheCurrentRouteMatches()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/wallet");

        var onWallet = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Wallet)
            .Add(component => component.Label, "Wallet")
            .Add(component => component.Href, "/wallet"));
        Assert.Equal("page", onWallet.Find("a").GetAttribute("aria-current"));

        var onDaily = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Daily)
            .Add(component => component.Label, "Daily")
            .Add(component => component.Href, "/daily"));
        Assert.Null(onDaily.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void RouteMode_PrefixMatchStaysActiveOnSubRoutes()
    {
        using var context = new BunitContext();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/account/reset-password");

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Account)
            .Add(component => component.Label, "Account")
            .Add(component => component.Href, "/account")
            .Add(component => component.Match, NavLinkMatch.Prefix));

        Assert.Equal("page", cut.Find("a").GetAttribute("aria-current"));
    }

    [Fact]
    public void RouteMode_InvokesOnNavigateWhenClicked()
    {
        using var context = new BunitContext();
        var navigated = false;

        var cut = context.Render<NavigationItem>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Daily)
            .Add(component => component.Label, "Daily")
            .Add(component => component.Href, "/daily")
            .Add(component => component.OnNavigate, () => navigated = true));

        cut.Find("a").Click();
        Assert.True(navigated);
    }
}
