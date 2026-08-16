using BeeDay.Web.Components.DesignSystem;
using BeeDay.Web.Localization;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Localization;

public sealed class ValidationMessageLocalizerTests
{
    [Theory]
    [InlineData("en-US", "Title is required.", "Title is required.")]
    [InlineData("pt-BR", "Title is required.", "O título é obrigatório.")]
    [InlineData("en-US", "Use a valid hexadecimal color.", "Use a valid hexadecimal color.")]
    [InlineData("pt-BR", "Use a valid hexadecimal color.", "Use uma cor hexadecimal válida.")]
    [InlineData("en-US", "Passwords do not match.", "Passwords do not match.")]
    [InlineData("pt-BR", "Passwords do not match.", "As senhas não coincidem.")]
    public void Translate_KnownMessage_ReturnsTheLocalizedText(string culture, string rawMessage, string expected)
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture(culture, () =>
            ValidationMessageLocalizer.Translate(rawMessage, localizer));

        Assert.Equal(expected, translated);
    }

    [Fact]
    public void Translate_UnknownMessage_IsReturnedUnchanged()
    {
        var localizer = CreateLocalizer();

        var translated = BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
            ValidationMessageLocalizer.Translate("The Amount field must be between 0.01 and X.", localizer));

        Assert.Equal("The Amount field must be between 0.01 and X.", translated);
    }

    private static IStringLocalizer<DesignSystemResources> CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<DesignSystemResources>>();
    }
}
