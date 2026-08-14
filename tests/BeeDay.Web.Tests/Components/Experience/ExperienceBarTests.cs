using BeeDay.Web.Components.Features.Experience.Components;
using BeeDay.Web.Components.Features.Experience.Models;

namespace BeeDay.Web.Tests.Components.Experience;

public sealed class ExperienceBarTests
{
    [Fact]
    public void RendersLoadingStateWithoutProgressBar()
    {
        using var context = new BunitContext();

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.IsLoading, true));

        Assert.Equal("true", cut.Find("[aria-label='Loading experience']").GetAttribute("aria-busy"));
        Assert.Equal(3, cut.FindAll(".experience-card__skeleton").Count);
        Assert.Empty(cut.FindAll("[role='progressbar']"));
    }

    [Fact]
    public void RendersLevelAndExperienceValues()
    {
        using var context = new BunitContext();
        var model = new ExperienceViewModel(4, 120, 200, 80);

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.Contains("Level", cut.Markup);
        Assert.Contains(">4<", cut.Markup);
        Assert.Contains("0 XP total", cut.Markup);
        Assert.Contains("120 / 200 XP", cut.Markup);
        Assert.Contains("80 XP to next level", cut.Markup);
        Assert.Contains("beeday-icon--size-large", cut.Find(".experience-card__header .beeday-icon").ClassList);
    }

    [Fact]
    public void RendersAccessibleProgressWithCorrectPercentage()
    {
        using var context = new BunitContext();
        var model = new ExperienceViewModel(2, 50, 200, 150);

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.Model, model));

        var progress = cut.Find("[role='progressbar']");
        Assert.Equal("50", progress.GetAttribute("aria-valuenow"));
        Assert.Equal("200", progress.GetAttribute("aria-valuemax"));
        Assert.Contains("width: 25%", cut.Find(".beeday-progress__fill").GetAttribute("style"));
    }

    [Theory]
    [InlineData(-25, 0)]
    [InlineData(250, 100)]
    public void ClampsProgressPercentage(long currentExperience, int expectedPercentage)
    {
        using var context = new BunitContext();
        var model = new ExperienceViewModel(3, currentExperience, 100, 0);

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.Contains(
            $"width: {expectedPercentage}%",
            cut.Find(".beeday-progress__fill").GetAttribute("style"));
    }

    [Fact]
    public void RendersExperienceGainFeedbackWhenRewardWasGranted()
    {
        using var context = new BunitContext();
        var model = new ExperienceViewModel(1, 10, 100, 90);

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.Model, model)
            .Add(component => component.ExperienceGain, 10)
            .Add(component => component.FeedbackKey, 1));

        var feedback = cut.Find(".experience-card__gain");
        Assert.Equal("polite", feedback.GetAttribute("aria-live"));
        Assert.Contains("+10 XP", feedback.TextContent);
    }

    [Fact]
    public void DoesNotRenderFeedbackWithoutNewReward()
    {
        using var context = new BunitContext();
        var model = new ExperienceViewModel(1, 10, 100, 90);

        var cut = context.Render<ExperienceBar>(parameters => parameters
            .Add(component => component.Model, model));

        Assert.Empty(cut.FindAll(".experience-card__gain"));
    }

    [Fact]
    public void RendersNothingWhenModelIsUnavailableAndNotLoading()
    {
        using var context = new BunitContext();

        var cut = context.Render<ExperienceBar>();

        Assert.DoesNotContain("experience-card", cut.Markup);
    }
}
