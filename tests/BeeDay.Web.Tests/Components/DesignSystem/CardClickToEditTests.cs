using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Dashboard.Components;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.DesignSystem;

public sealed class CardClickToEditTests
{
    [Fact]
    public void ActivityCard_ClickOnBody_InvokesOnEdit()
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task")
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find("[role='button']").Click();

        Assert.True(editInvoked);
    }

    [Fact]
    public void ActivityCard_ClickOnToggle_DoesNotInvokeOnEdit()
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var toggleInvoked = false;
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task")
            .Add(component => component.OnToggle, () => toggleInvoked = true)
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find(".activity-card__checkbox").Click();

        Assert.True(toggleInvoked);
        Assert.False(editInvoked);
    }

    [Theory]
    [InlineData(false, "Complete Read chapter")]
    [InlineData(true, "Mark Read chapter as incomplete")]
    public void ActivityCard_Checkbox_HasMatchingTitleAndAriaLabel(bool completed, string expectedLabel)
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task")
            .Add(component => component.Completed, completed)
            .Add(component => component.OnToggle, () => { })));

        var checkbox = cut.Find(".activity-card__checkbox");

        Assert.Equal(expectedLabel, checkbox.GetAttribute("title"));
        Assert.Equal(expectedLabel, checkbox.GetAttribute("aria-label"));
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void ActivityCard_ActivationKeyOnBody_InvokesOnEdit(string key)
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task")
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find("[role='button']").KeyDown(key);

        Assert.True(editInvoked);
    }

    [Fact]
    public void ActivityCard_OtherKeyOnBody_DoesNotInvokeOnEdit()
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task")
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find("[role='button']").KeyDown("Tab");

        Assert.False(editInvoked);
    }

    [Fact]
    public void ActivityCard_ExposesAccessibleEditName()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "todo")));

        Assert.Equal("Edit To-Do: Read chapter", cut.Find("[role='button']").GetAttribute("aria-label"));
    }

    [Fact]
    public void ActivityCard_DoesNotRenderLegacyMenuTrigger()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task"));

        Assert.DoesNotContain("activity-card__menu", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ActivityCard_OnlyClickedInstanceInvokesItsOwnOnEdit()
    {
        using var contextA = new BunitContext().WithLocalization();
        using var contextB = new BunitContext().WithLocalization();
        var editedA = false;
        var editedB = false;

        var cutA = contextA.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Card A")
            .Add(component => component.Description, string.Empty)
            .Add(component => component.Variant, "task")
            .Add(component => component.OnEdit, () => editedA = true));

        contextB.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Card B")
            .Add(component => component.Description, string.Empty)
            .Add(component => component.Variant, "task")
            .Add(component => component.OnEdit, () => editedB = true));

        cutA.Find("[role='button']").Click();

        Assert.True(editedA);
        Assert.False(editedB);
    }

    [Fact]
    public void HabitCard_ClickOnBody_InvokesOnEdit()
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both)
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find("[role='button']").Click();

        Assert.True(editInvoked);
    }

    [Fact]
    public void HabitCard_ClickOnScoreButtons_DoesNotInvokeOnEdit()
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var positiveInvoked = false;
        var negativeInvoked = false;
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both)
            .Add(component => component.OnPositive, () => positiveInvoked = true)
            .Add(component => component.OnNegative, () => negativeInvoked = true)
            .Add(component => component.OnEdit, () => editInvoked = true));

        var scoreButtons = cut.FindAll(".habit-card__score-button");
        scoreButtons[0].Click();
        scoreButtons[1].Click();

        Assert.True(positiveInvoked);
        Assert.True(negativeInvoked);
        Assert.False(editInvoked);
    }

    [Theory]
    [InlineData("Enter")]
    [InlineData(" ")]
    public void HabitCard_ActivationKeyOnBody_InvokesOnEdit(string key)
    {
        using var context = new BunitContext().WithLocalization();
        var editInvoked = false;
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both)
            .Add(component => component.OnEdit, () => editInvoked = true));

        cut.Find("[role='button']").KeyDown(key);

        Assert.True(editInvoked);
    }

    [Fact]
    public void HabitCard_ExposesAccessibleEditName()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both)));

        Assert.Equal("Edit Habit: Meditate", cut.Find("[role='button']").GetAttribute("aria-label"));
    }

    [Fact]
    public void HabitCard_DoesNotRenderLegacyMenuTrigger()
    {
        using var context = new BunitContext().WithLocalization();
        var cut = context.Render<HabitCard>(parameters => parameters
            .Add(component => component.Title, "Meditate")
            .Add(component => component.Direction, HabitDirection.Both));

        Assert.DoesNotContain("habit-card__menu", cut.Markup, StringComparison.Ordinal);
    }
}
