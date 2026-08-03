using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Services;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class BeeDayToastHostTests
{
    [Fact]
    public void RendersSuccessToastFromService()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        var cut = context.Render<BeeDayToastHost>();

        cut.InvokeAsync(() => service.ShowSuccess("Habit saved", "Saved"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast--success")));

        var toast = cut.Find(".beeday-toast--success");
        Assert.Equal("status", toast.GetAttribute("role"));
        Assert.Contains("Saved", toast.TextContent);
        Assert.Contains("Habit saved", toast.TextContent);
        Assert.NotNull(toast.QuerySelector("svg.pixel-icon--success"));
    }

    [Fact]
    public void ErrorToastUsesAlertRole()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        var cut = context.Render<BeeDayToastHost>();

        cut.InvokeAsync(() => service.ShowError("Unable to save"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast--error")));

        Assert.Equal("alert", cut.Find(".beeday-toast--error").GetAttribute("role"));
        Assert.NotNull(cut.Find("svg.pixel-icon--validation-error"));
    }

    [Fact]
    public void DismissButtonRemovesToast()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        var cut = context.Render<BeeDayToastHost>();

        cut.InvokeAsync(() => service.ShowInfo("Information"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".beeday-toast")));
        cut.Find(".beeday-toast__close").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".beeday-toast")));
        Assert.Empty(service.Messages);
    }
}
