using BeeDay.Application.Common.Events;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Events;
using BeeDay.Web.Components.Features.Experience.Feedback;

namespace BeeDay.Web.Tests.Components.Experience;

public sealed class BeeDayFeedbackTests
{
    [Fact]
    public void DoesNotRenderWithoutBeeDayFeedback()
    {
        using var context = CreateContext();

        var cut = context.Render<BeeDayFeedbackModal>();

        Assert.Empty(cut.FindAll("[role='dialog']"));
    }

    [Fact]
    public void RendersSingleLevelTransitionAndSource()
    {
        using var context = CreateContext();
        BeeDayFeedback feedback = CreateFeedback(4, 5, 20, ExperienceSourceType.Project);

        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback]));

        Assert.Equal("4", cut.Find("[data-testid='previous-level']").TextContent);
        Assert.Equal("5", cut.Find("[data-testid='new-level']").TextContent);
        Assert.Equal("1", cut.Find("[data-testid='levels-gained']").TextContent);
        Assert.Contains("+20 XP from Project Completed", cut.Find("[data-testid='experience-source']").TextContent);
    }

    [Fact]
    public void RendersOneFeedbackForMultipleLevels()
    {
        using var context = CreateContext();
        BeeDayFeedback feedback = CreateFeedback(3, 7, 80, ExperienceSourceType.Task);

        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback]));

        Assert.Single(cut.FindAll("[role='dialog']"));
        Assert.Equal("4", cut.Find("[data-testid='levels-gained']").TextContent);
        Assert.Contains("advanced 4 levels", cut.Markup);
    }

    [Fact]
    public void CloseButtonInvokesCallback()
    {
        using var context = CreateContext();
        var closed = false;
        BeeDayFeedback feedback = CreateFeedback(1, 2, 10, ExperienceSourceType.Habit);
        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback])
            .Add(component => component.OnClose, () => closed = true));

        cut.Find("button").Click();

        Assert.True(closed);
    }

    [Fact]
    public void EscapeKeyInvokesCallback()
    {
        using var context = CreateContext();
        var closed = false;
        BeeDayFeedback feedback = CreateFeedback(1, 2, 10, ExperienceSourceType.Habit);
        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback])
            .Add(component => component.OnClose, () => closed = true));

        cut.Find(".beeday-feedback-backdrop").KeyDown("Escape");

        Assert.True(closed);
    }

    [Fact]
    public void IncludesAccessibleDialogAttributes()
    {
        using var context = CreateContext();
        BeeDayFeedback feedback = CreateFeedback(1, 2, 10, ExperienceSourceType.Habit);
        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback]));

        var dialog = cut.Find("[role='dialog']");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("beeday-feedback-title", dialog.GetAttribute("aria-labelledby"));
        Assert.Equal("beeday-feedback-description", dialog.GetAttribute("aria-describedby"));
        Assert.True(dialog.HasAttribute("tabindex"));
    }

    [Fact]
    public void AppliesThePixelAdapterToThePanelAndContinueAction()
    {
        using var context = CreateContext();
        BeeDayFeedback feedback = CreateFeedback(1, 2, 10, ExperienceSourceType.Habit);
        var cut = context.Render<BeeDayFeedbackModal>(parameters => parameters
            .Add(component => component.Feedback, feedback)
            .Add(component => component.History, [feedback]));

        var dialog = cut.Find("[role='dialog']");
        Assert.Contains("beeday-pixel-panel", dialog.ClassList);
        Assert.Contains("beeday-button--primary", cut.Find("button").ClassList);
        Assert.DoesNotContain("beeday-pixel-cta", cut.Find("button").ClassList);
    }

    [Fact]
    public async Task HandlerIgnoresExperienceWithoutLevelUp()
    {
        var store = new BeeDayFeedbackStore();
        var handler = new BeeDayFeedbackEventHandler(store);
        var grantedEvent = new ExperienceGrantedDomainEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            ExperienceSourceType.Habit,
            Guid.NewGuid(),
            ExperienceRewardType.Completion,
            DateTimeOffset.UtcNow);

        await handler.Handle(new DomainEventNotification(grantedEvent), CancellationToken.None);

        Assert.Null(store.Current);
        Assert.Empty(store.History);
    }

    [Fact]
    public void StoreRejectsDuplicateExperienceEntryAndConsumesFeedback()
    {
        var store = new BeeDayFeedbackStore();
        BeeDayFeedback feedback = CreateFeedback(2, 3, 15, ExperienceSourceType.Todo);

        store.Add(feedback);
        store.Add(feedback with { EventId = Guid.NewGuid() });

        Assert.Single(store.History);
        Assert.NotNull(store.Current);

        store.Consume();

        Assert.Null(store.Current);
    }

    [Fact]
    public void HostDoesNotReappearAfterFeedbackIsConsumed()
    {
        using var context = CreateContext();
        var store = new BeeDayFeedbackStore();
        context.Services.AddSingleton(store);
        var cut = context.Render<BeeDayFeedbackHost>();

        store.Add(CreateFeedback(5, 6, 20, ExperienceSourceType.Project));
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[role='dialog']")));

        cut.Find("button").Click();
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll("[role='dialog']")));
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private static BeeDayFeedback CreateFeedback(
        int previousLevel,
        int newLevel,
        long amount,
        ExperienceSourceType source) =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            previousLevel,
            newLevel,
            newLevel - previousLevel,
            amount,
            source,
            DateTimeOffset.UtcNow);
}
