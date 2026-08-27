using BeeDay.Web.Components.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Pages;

public sealed class NotFoundTests
{
    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishCopy()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<NotFound>());

        Assert.Equal("Not Found", cut.Find("h1").TextContent);
        Assert.Equal("Sorry, the content you are looking for does not exist.", cut.Find("p").TextContent);
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseCopy()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<NotFound>());

        Assert.Equal("Não encontrado", cut.Find("h1").TextContent);
        Assert.Equal("O conteúdo que você está procurando não existe.", cut.Find("p").TextContent);
    }
}
