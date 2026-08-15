using System.Globalization;
using BeeDay.Web.Resources;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Localization;

/// <summary>
/// Exercises IStringLocalizer&lt;SharedResources&gt; resolution directly (no HTTP pipeline), one
/// layer below RequestLocalizationIntegrationTests — this proves the resx catalog itself resolves
/// correctly per culture; the integration tests prove the request pipeline only ever hands it a
/// supported culture.
/// </summary>
public sealed class SharedResourcesTests
{
    [Theory]
    [InlineData("en-US", "Be Better Every Day")]
    [InlineData("pt-BR", "Seja melhor a cada dia")]
    public void FooterTagline_ResolvesPerSupportedCulture(string culture, string expected)
    {
        var localizer = CreateLocalizer();

        WithUiCulture(culture, () =>
        {
            var value = localizer["FooterTagline"];

            Assert.Equal(expected, value.Value);
            Assert.False(value.ResourceNotFound);
        });
    }

    [Fact]
    public void UnsupportedCulture_FallsBackToTheNeutralEnglishResource()
    {
        var localizer = CreateLocalizer();

        WithUiCulture("fr-FR", () =>
        {
            var value = localizer["FooterTagline"];

            Assert.Equal("Be Better Every Day", value.Value);
            Assert.False(value.ResourceNotFound);
        });
    }

    private static IStringLocalizer<SharedResources> CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        return services.BuildServiceProvider().GetRequiredService<IStringLocalizer<SharedResources>>();
    }

    private static void WithUiCulture(string culture, Action action)
    {
        var restore = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = restore;
        }
    }
}
