using LevelUp.Web.Components.DesignSystem.Feedback;

namespace LevelUp.Web.Tests.Components.Feedback;

public sealed class LevelUpConfirmDialogTests
{
    [Fact]
    public void DoesNotRenderWhenClosed()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?"));

        Assert.Empty(cut.FindAll("[role='alertdialog']"));
    }

    [Fact]
    public void RendersContentAndOptionalWarningWhenOpen()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete task")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.ItemTitle, "Study bUnit")
            .Add(component => component.Warning, "This cannot be undone")
            .Add(component => component.WarningDetails, "The task will be removed permanently."));

        var dialog = cut.Find("[role='alertdialog']");
        Assert.Contains("Delete task", dialog.TextContent);
        Assert.Contains("Study bUnit", dialog.TextContent);
        Assert.Contains("This cannot be undone", dialog.TextContent);
    }

    [Fact]
    public void ConfirmAndCancelInvokeCallbacks()
    {
        using var context = new BunitContext();
        var confirmed = false;
        var cancelled = false;
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.OnConfirm, () => confirmed = true)
            .Add(component => component.OnCancel, () => cancelled = true));

        cut.Find(".delete-confirmation__confirm-action").Click();
        cut.Find(".delete-confirmation__cancel-action").Click();

        Assert.True(confirmed);
        Assert.True(cancelled);
    }

    [Fact]
    public void BusyStateDisablesActionsAndSuppressesCallbacks()
    {
        using var context = new BunitContext();
        var callbackCount = 0;
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsBusy, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.OnConfirm, () => callbackCount++)
            .Add(component => component.OnCancel, () => callbackCount++));

        foreach (var button in cut.FindAll("button"))
        {
            Assert.True(button.HasAttribute("disabled"));
        }

        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void EscapeKeyInvokesCancel()
    {
        using var context = new BunitContext();
        var cancelled = false;
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.OnCancel, () => cancelled = true));

        cut.Find(".delete-confirmation-backdrop").KeyDown("Escape");

        Assert.True(cancelled);
    }

    [Fact]
    public void UsesPixelIconsForDeleteWarningAndActions()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete item")
            .Add(component => component.Message, "Confirm deletion")
            .Add(component => component.Warning, "Cannot be undone"));

        Assert.NotNull(cut.Find("svg.pixel-icon--delete"));
        Assert.NotNull(cut.Find("svg.pixel-icon--warning"));
        Assert.NotNull(cut.Find("svg.pixel-icon--cancel"));
    }

    [Fact]
    public void RendersStandardizedSideBySideActions()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete project")
            .Add(component => component.Message, "Are you sure?"));

        var actions = cut.Find(".delete-confirmation__actions");
        Assert.NotNull(actions.QuerySelector(".delete-confirmation__cancel-action"));
        Assert.NotNull(actions.QuerySelector(".delete-confirmation__confirm-action"));
    }
}
