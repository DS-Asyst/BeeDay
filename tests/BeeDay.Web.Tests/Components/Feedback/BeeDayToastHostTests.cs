using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Services;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class BeeDayToastHostTests
{
    [Fact]
    public void RendersSuccessToastFromService()
    {
        using var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<ToastService>();
        var cut = context.Render<BeeDayToastHost>();
        var service = context.Services.GetRequiredService<ToastService>();

        cut.InvokeAsync(() => service.ShowSuccess("Habit saved", "Saved"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast--success")));

        var toast = cut.Find(".beeday-toast--success");
        Assert.Equal("status", toast.GetAttribute("role"));
        Assert.Contains("Saved", toast.TextContent);
        Assert.Contains("Habit saved", toast.TextContent);
        Assert.NotNull(toast.QuerySelector("svg.beeday-icon--success"));
    }

    [Fact]
    public void ErrorToastUsesAlertRole()
    {
        using var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<ToastService>();
        var cut = context.Render<BeeDayToastHost>();
        var service = context.Services.GetRequiredService<ToastService>();

        cut.InvokeAsync(() => service.ShowError("Unable to save"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast--error")));

        Assert.Equal("alert", cut.Find(".beeday-toast--error").GetAttribute("role"));
        Assert.NotNull(cut.Find("svg.beeday-icon--validation-error"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_RendersPortugueseChromeAriaLabels()
    {
        using var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<ToastService>();
        var service = context.Services.GetRequiredService<ToastService>();

        // The toast host re-renders on ToastService.Changed (a new toast arriving re-executes the
        // whole component, re-evaluating every @Localizer[...] expression, not just the new toast's
        // own markup — so the region's aria-label is just as culture-sensitive on this second render
        // as the close button's), so the culture pin has to stay active through that re-render, not
        // just the initial one.
        BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
        {
            var cut = context.Render<BeeDayToastHost>();

            cut.InvokeAsync(() => service.ShowInfo("Sincronização concluída"));
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast")));

            Assert.Equal("Notificações", cut.Find(".beeday-toast-region").GetAttribute("aria-label"));
            Assert.Equal("region", cut.Find(".beeday-toast-region").GetAttribute("role"));
            Assert.Equal("Dispensar notificação", cut.Find(".beeday-toast__close").GetAttribute("aria-label"));
        });
    }

    [Fact]
    public void DismissButtonRemovesToast()
    {
        using var context = new BunitContext().WithLocalization();
        context.Services.AddSingleton<ToastService>();
        var cut = context.Render<BeeDayToastHost>();
        var service = context.Services.GetRequiredService<ToastService>();

        cut.InvokeAsync(() => service.ShowInfo("Information"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast")));
        cut.Find(".beeday-toast__close").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".beeday-toast")));
        Assert.Empty(service.Messages);
    }
}
