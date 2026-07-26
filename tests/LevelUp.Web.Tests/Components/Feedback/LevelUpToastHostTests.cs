using LevelUp.Web.Components.DesignSystem.Feedback;
using LevelUp.Web.Services;

namespace LevelUp.Web.Tests.Components.Feedback;

public sealed class LevelUpToastHostTests
{
    [Fact]
    public void RendersSuccessToastFromService()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        var cut = context.Render<LevelUpToastHost>();

        cut.InvokeAsync(() => service.ShowSuccess("Habit saved", "Saved"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".levelup-toast--success")));

        var toast = cut.Find(".levelup-toast--success");
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
        var cut = context.Render<LevelUpToastHost>();

        cut.InvokeAsync(() => service.ShowError("Unable to save"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".levelup-toast--error")));

        Assert.Equal("alert", cut.Find(".levelup-toast--error").GetAttribute("role"));
        Assert.NotNull(cut.Find("svg.pixel-icon--validation-error"));
    }

    [Fact]
    public void DismissButtonRemovesToast()
    {
        using var context = new BunitContext();
        var service = new ToastService();
        context.Services.AddSingleton(service);
        var cut = context.Render<LevelUpToastHost>();

        cut.InvokeAsync(() => service.ShowInfo("Information"));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".levelup-toast")));
        cut.Find(".levelup-toast__close").Click();

        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".levelup-toast")));
        Assert.Empty(service.Messages);
    }
}
