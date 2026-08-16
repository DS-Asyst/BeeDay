using BeeDay.Web.Components.DesignSystem.Progress;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class BeeDayProgressBarTests
{
    [Theory]
    [InlineData(0, 100, "empty", "width: 0%")]
    [InlineData(25, 100, "partial", "width: 25%")]
    [InlineData(100, 100, "complete", "width: 100%")]
    [InlineData(150, 100, "complete", "width: 100%")]
    [InlineData(20, 0, "unavailable", "width: 0%")]
    public void RepresentsProgressStatesSafely(double value, double maximum, string state, string width)
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Task completion")
            .Add(component => component.Value, value)
            .Add(component => component.Maximum, maximum));

        var root = cut.Find(".beeday-progress");
        var progress = cut.Find("[role='progressbar']");
        Assert.Equal(state, root.GetAttribute("data-state"));
        Assert.Equal("Task completion", progress.GetAttribute("aria-label"));
        Assert.Equal("0", progress.GetAttribute("aria-valuemin"));
        Assert.Contains(width, cut.Find(".beeday-progress__fill").GetAttribute("style"));
    }

    [Fact]
    public void ExposesClampedAccessibleValuesAndVisibleLabel()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Project task completion")
            .Add(component => component.Value, 12)
            .Add(component => component.Maximum, 10)
            .Add(component => component.ValueText, "10 of 10 completed"));

        var progress = cut.Find("[role='progressbar']");
        Assert.Contains("Project task completion", cut.Markup);
        Assert.Equal("10", progress.GetAttribute("aria-valuenow"));
        Assert.Equal("10", progress.GetAttribute("aria-valuemax"));
        Assert.Equal("10 of 10 completed", progress.GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void UnderEnglishUiCulture_DefaultsUnsetValueTextToEnglish()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Task completion")
            .Add(component => component.Value, 5)
            .Add(component => component.Maximum, 10)));

        Assert.Equal("5 of 10", cut.Find("[role='progressbar']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_DefaultsUnsetValueTextToPortuguese()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Task completion")
            .Add(component => component.Value, 5)
            .Add(component => component.Maximum, 10)));

        Assert.Equal("5 de 10", cut.Find("[role='progressbar']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void UnderPortugueseUiCulture_UnavailableMaximumRendersPortugueseFallbackText()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Task completion")
            .Add(component => component.Value, 5)
            .Add(component => component.Maximum, 0)));

        Assert.Equal("Progresso indisponível", cut.Find("[role='progressbar']").GetAttribute("aria-valuetext"));
    }

    [Fact]
    public void ExposesRewardToneWithoutChangingProgressSemantics()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<BeeDayProgressBar>(parameters => parameters
            .Add(component => component.Label, "Experience progress")
            .Add(component => component.Value, 25)
            .Add(component => component.Maximum, 100)
            .Add(component => component.Tone, BeeDayProgressTone.Reward));

        Assert.Equal("reward", cut.Find(".beeday-progress").GetAttribute("data-tone"));
        Assert.Equal("25", cut.Find("[role='progressbar']").GetAttribute("aria-valuenow"));
    }
}
