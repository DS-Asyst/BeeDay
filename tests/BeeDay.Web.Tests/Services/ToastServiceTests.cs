using System.Globalization;
using BeeDay.Web.Resources;
using BeeDay.Web.Services;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Tests.Services;

/// <summary>
/// Covers the culture-aware default toast titles (Success/Error/Information) that apply when a
/// caller doesn't pass an explicit title — mirrors the same default/override contract exercised
/// for the Design System's shared components in BeeDayConfirmDialogTests/FeedbackComponentTests.
/// </summary>
public sealed class ToastServiceTests
{
    private static ToastService CreateService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();
        return new ToastService(provider.GetRequiredService<IStringLocalizer<SharedResources>>());
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

    [Fact]
    public void UnderEnglishUiCulture_DefaultsUnsetTitlesToEnglish()
    {
        var service = CreateService();

        WithUiCulture("en-US", () =>
        {
            service.ShowSuccess("Habit saved");
            service.ShowError("Unable to save");
            service.ShowInfo("Sync complete");
        });

        Assert.Equal(["Success", "Something went wrong", "Information"], service.Messages.Select(message => message.Title));
    }

    [Fact]
    public void UnderPortugueseUiCulture_DefaultsUnsetTitlesToPortuguese()
    {
        var service = CreateService();

        WithUiCulture("pt-BR", () =>
        {
            service.ShowSuccess("Hábito salvo");
            service.ShowError("Não foi possível salvar");
            service.ShowInfo("Sincronização concluída");
        });

        Assert.Equal(["Sucesso", "Algo deu errado", "Informação"], service.Messages.Select(message => message.Title));
    }

    [Fact]
    public void ExplicitTitleStillOverridesTheCultureAwareDefault()
    {
        var service = CreateService();

        WithUiCulture("pt-BR", () => service.ShowSuccess("Hábito salvo", "Tudo certo"));

        Assert.Equal("Tudo certo", service.Messages.Single().Title);
    }
}
