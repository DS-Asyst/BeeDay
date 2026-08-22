using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class NavigationItemsTests
{
    [Fact]
    public void SharedSourceContainsOnlyRealRoutesAndSecureLogout()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<NavigationItems>());
        var links = cut.FindAll("a.navigation-item");
        Assert.Equal(["/profile", "/daily", "/wallet", "/settings"], links.Select(link => link.GetAttribute("href")));
        var logout = cut.Find("form[method='post'][action='/auth/logout'] button[type='submit']");
        Assert.Equal("Log out of beeday", logout.GetAttribute("aria-label"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseLabels()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<NavigationItems>());

        var labels = cut.FindAll("a.navigation-item .navigation-item__label");
        Assert.Equal(["Perfil", "Diário", "Carteira", "Conta"], labels.Select(label => label.TextContent));
        var logout = cut.Find("form[method='post'][action='/auth/logout'] button[type='submit']");
        Assert.Equal("Sair do beeday", logout.GetAttribute("aria-label"));
    }

    [Fact]
    public void AccountItemIsScopedForItsOwnAccentColorSeparatelyFromLogout()
    {
        // EPIC 27 Sprint 27.9: Account gets a dedicated wrapper (mirroring the logout form's own
        // wrapper) so its resting-state COR3 accent (NavigationItems.razor.css) can be scoped
        // without touching Profile/Daily/Wallet or Logout's own colors.
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<NavigationItems>();

        var accountLink = cut.Find(".navigation-items__account a.navigation-item[href='/settings']");
        Assert.NotNull(accountLink);
    }

    [Fact]
    public void EveryRouteInvokesMobileCloseCallback()
    {
        using var context = new BunitContext().WithLocalization();
        var count = 0;
        var cut = context.Render<NavigationItems>(parameters => parameters.Add(component => component.OnNavigate, () => count++));
        foreach (var link in cut.FindAll("a.navigation-item"))
        {
            link.Click();
        }
        Assert.Equal(4, count);
    }
}
