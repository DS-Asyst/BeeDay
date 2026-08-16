using BeeDay.Web.Components.Pages;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Pages;

public sealed class ErrorTests
{
    [Theory]
    [InlineData("en-US", "Error", "Development mode", "For local debugging")]
    [InlineData("pt-BR", "Erro", "Modo de desenvolvimento", "Para depuração local")]
    public void RendersOperationalAndDevelopmentCopyForTheActiveCulture(
        string culture,
        string expectedTitle,
        string expectedDevelopmentHeading,
        string expectedInstructions)
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => context.Render<Error>());

        Assert.Equal(expectedTitle, cut.Find("h1").TextContent.Trim());
        Assert.Equal(expectedDevelopmentHeading, cut.Find("h3").TextContent.Trim());
        Assert.Contains(expectedInstructions, cut.Markup, StringComparison.Ordinal);
        Assert.Contains("ASPNETCORE_ENVIRONMENT", cut.Markup, StringComparison.Ordinal);
    }
}
