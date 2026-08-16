using BeeDay.Web.Tests.Localization;
using WelcomePage = BeeDay.Web.Components.Features.ProfileCreation.Pages.Welcome;

namespace BeeDay.Web.Tests.Components.Onboarding;

public sealed class WelcomeTests : BunitContext
{
    public WelcomeTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

    [Fact]
    public void UnderEnglishUiCulture_RendersTheEnglishRedirectMessage()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<WelcomePage>());

        Assert.Contains("Redirecting to login...", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersThePortugueseRedirectMessage()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<WelcomePage>());

        Assert.Contains("Redirecionando para o login...", cut.Markup, StringComparison.Ordinal);
        Assert.DoesNotContain("Redirecting to login", cut.Markup, StringComparison.Ordinal);
    }
}
