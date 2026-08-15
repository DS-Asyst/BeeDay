using BeeDay.Web.Components.Layout;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Layout;

public sealed class ReconnectModalTests
{
    [Fact]
    public void UnderEnglishUiCulture_RendersEnglishCopy()
    {
        using var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ReconnectModal>());

        Assert.Contains("Rejoining the server...", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Failed to rejoin.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Please retry or reload the page.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains(">Retry<", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("The session has been paused by the server.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Failed to resume the session.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains(">Resume<", cut.Markup, StringComparison.Ordinal);
        Assert.NotNull(cut.Find("#components-seconds-to-next-attempt"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseCopy()
    {
        using var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<ReconnectModal>());

        Assert.Contains("Reconectando ao servidor...", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Falha ao reconectar.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Tente novamente ou recarregue a página.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("A sessão foi pausada pelo servidor.", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Falha ao retomar a sessão.", cut.Markup, StringComparison.Ordinal);
        Assert.NotNull(cut.Find("#components-seconds-to-next-attempt"));
    }
}
