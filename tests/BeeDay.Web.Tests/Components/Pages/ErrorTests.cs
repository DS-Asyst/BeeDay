using BeeDay.Web.Components.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Pages;

public sealed class ErrorTests
{
    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishCopy()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<Error>());

        Assert.Equal("Error.", cut.Find("h1").TextContent);
        Assert.Equal("An error occurred while processing your request.", cut.Find("h2").TextContent);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseCopy()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<Error>());

        Assert.Equal("Erro.", cut.Find("h1").TextContent);
        Assert.Equal("Ocorreu um erro ao processar sua solicitação.", cut.Find("h2").TextContent);
    }
}
