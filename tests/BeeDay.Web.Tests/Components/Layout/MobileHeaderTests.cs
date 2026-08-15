using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class MobileHeaderTests
{
    [Fact]
    public void RendersBrandLinkToDaily()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false));

        var brand = cut.Find("a.mobile-header__brand");
        Assert.Equal("/profile", brand.GetAttribute("href"));
    }

    [Fact]
    public void ClosedState_HasCorrectAriaContract()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false)));

        var button = cut.Find(".mobile-header__menu-button");
        Assert.Equal("Open navigation menu", button.GetAttribute("aria-label"));
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
        Assert.Equal("mobile-navigation", button.GetAttribute("aria-controls"));
    }

    [Fact]
    public void OpenState_HasCorrectAriaContract()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, true)));

        var button = cut.Find(".mobile-header__menu-button");
        Assert.Equal("Close navigation menu", button.GetAttribute("aria-label"));
        Assert.Equal("true", button.GetAttribute("aria-expanded"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseAriaLabels()
    {
        using var context = new BunitContext().WithLocalization();

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false)));

        var brand = cut.Find("a.mobile-header__brand");
        var button = cut.Find(".mobile-header__menu-button");
        Assert.Equal("BeeDay — ir para o Perfil", brand.GetAttribute("aria-label"));
        Assert.Equal("Abrir menu de navegação", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void MenuButtonClick_InvokesOnToggleNav()
    {
        using var context = new BunitContext().WithLocalization();
        var toggled = false;

        var cut = context.Render<MobileHeader>(parameters => parameters
            .Add(component => component.IsNavOpen, false)
            .Add(component => component.OnToggleNav, () => toggled = true));

        cut.Find(".mobile-header__menu-button").Click();
        Assert.True(toggled);
    }
}
