using BeeDay.Web.Components.Layout;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class PublicHeaderTests
{
    [Fact]
    public void RendersHeaderLandmarkWithBrandAndLoginCtaForAnonymousUser()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();

        var cut = context.Render<PublicHeader>();

        Assert.NotNull(cut.Find("header.public-header"));
        Assert.NotNull(cut.Find(".public-header__brand .beeday-brand"));

        var cta = cut.Find("button");
        Assert.Equal("Log in", cta.TextContent.Trim());
    }

    [Fact]
    public void RendersDailyCtaForAuthenticatedUser()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");

        var cut = context.Render<PublicHeader>();

        var cta = cut.Find("button");
        Assert.Equal("Go to Daily", cta.TextContent.Trim());
    }

    [Fact]
    public void LoginCtaNavigatesToLogin()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();

        var cut = context.Render<PublicHeader>();
        cut.Find("button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/login", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void DailyCtaNavigatesToDaily()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetAuthorized("test-user");

        var cut = context.Render<PublicHeader>();
        cut.Find("button").Click();

        var navigation = context.Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/daily", navigation.Uri, StringComparison.Ordinal);
    }

    [Fact]
    public void BrandLinksHome()
    {
        using var context = new BunitContext();
        context.AddAuthorization().SetNotAuthorized();

        var cut = context.Render<PublicHeader>();

        var brandLink = cut.Find("a.public-header__brand");
        Assert.Equal("/", brandLink.GetAttribute("href"));
    }
}
