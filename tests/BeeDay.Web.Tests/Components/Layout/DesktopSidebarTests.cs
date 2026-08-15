using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class DesktopSidebarTests
{
    [Fact]
    public void ComposesOfficialBrandPrimaryRoutesAndSecondaryAccountActions()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<DesktopSidebar>();

        Assert.Equal("/profile", cut.Find("a.desktop-sidebar__brand-link").GetAttribute("href"));
        var primary = cut.FindAll("nav.navigation-items a.navigation-item");
        Assert.Equal(["/profile", "/daily", "/wallet"], primary.Select(link => link.GetAttribute("href")));
        Assert.NotNull(cut.Find(".navigation-items--actions a[href='/settings']"));
        Assert.Equal("/auth/logout", cut.Find("form.navigation-items__logout-form").GetAttribute("action"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseAriaLabels()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<DesktopSidebar>());

        Assert.Equal("Navegação principal", cut.Find("aside.desktop-sidebar").GetAttribute("aria-label"));
        Assert.Equal("BeeDay — ir para o Perfil", cut.Find("a.desktop-sidebar__brand-link").GetAttribute("aria-label"));
    }

    [Fact]
    public void MarksCurrentRouteWithAriaCurrent()
    {
        using var context = new BunitContext().WithLocalization();
        context.Services.GetRequiredService<NavigationManager>().NavigateTo("/wallet");
        var cut = context.Render<DesktopSidebar>();
        Assert.Equal("page", cut.Find("a[href='/wallet']").GetAttribute("aria-current"));
        Assert.Null(cut.Find("a[href='/daily']").GetAttribute("aria-current"));
    }
}
