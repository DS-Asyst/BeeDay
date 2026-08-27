using BeeDay.Web.Components.DesignSystem.Feedback;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Feedback;

public sealed class BeeDayConfirmDialogTests
{
    [Fact]
    public void DoesNotRenderWhenClosed()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?"));

        Assert.Empty(cut.FindAll("[role='alertdialog']"));
    }

    [Fact]
    public void RendersContentAndOptionalWarningWhenOpen()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
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
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("false", dialog.GetAttribute("aria-busy"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.NotNull(cut.Find($"#{dialog.GetAttribute("aria-labelledby")}"));
        Assert.NotNull(cut.Find($"#{dialog.GetAttribute("aria-describedby")}"));
    }

    [Fact]
    public void ConfirmAndCancelInvokeCallbacks()
    {
        using var context = CreateContext();
        var confirmed = false;
        var cancelled = false;
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
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
        using var context = CreateContext();
        var callbackCount = 0;
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
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

        Assert.Equal("true", cut.Find("[role='alertdialog']").GetAttribute("aria-busy"));
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void EscapeKeyInvokesCancel()
    {
        using var context = CreateContext();
        var cancelled = false;
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.OnCancel, () => cancelled = true));

        cut.Find(".delete-confirmation-backdrop").KeyDown("Escape");

        Assert.True(cancelled);
    }

    [Fact]
    public void UsesBeeDayIconsForDeleteAndWarningButNotForCancelOrConfirmButtons()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete item")
            .Add(component => component.Message, "Confirm deletion")
            .Add(component => component.Warning, "Cannot be undone"));

        Assert.NotNull(cut.Find("svg.beeday-icon--delete"));
        Assert.NotNull(cut.Find("svg.beeday-icon--warning"));
        Assert.Empty(cut.Find(".delete-confirmation__cancel-action").QuerySelectorAll("svg"));
        Assert.Empty(cut.Find(".delete-confirmation__confirm-action").QuerySelectorAll("svg"));
    }

    [Fact]
    public void UnderEnglishUiCulture_DefaultsUnsetLabelsToEnglish()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")));

        Assert.Equal("Cancel", cut.Find(".delete-confirmation__cancel-action").TextContent.Trim());
        Assert.Equal("Confirm", cut.Find(".delete-confirmation__confirm-action").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_DefaultsUnsetLabelsToPortuguese()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete")
            .Add(component => component.Message, "Are you sure?")));

        Assert.Equal("Cancelar", cut.Find(".delete-confirmation__cancel-action").TextContent.Trim());
        Assert.Equal("Confirmar", cut.Find(".delete-confirmation__confirm-action").TextContent.Trim());
    }

    [Fact]
    public void ExplicitLabelsStillOverrideTheCultureAwareDefault()
    {
        using var context = CreateContext();
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete transaction")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.ConfirmLabel, "Delete transaction")
            .Add(component => component.CancelLabel, "Keep it")));

        Assert.Equal("Keep it", cut.Find(".delete-confirmation__cancel-action").TextContent.Trim());
        Assert.Equal("Delete transaction", cut.Find(".delete-confirmation__confirm-action").TextContent.Trim());
    }

    [Fact]
    public void RendersStandardizedSideBySideActions()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete project")
            .Add(component => component.Message, "Are you sure?"));

        var actions = cut.Find(".delete-confirmation__actions");
        Assert.NotNull(actions.QuerySelector(".delete-confirmation__cancel-action"));
        Assert.NotNull(actions.QuerySelector(".delete-confirmation__confirm-action"));
    }

    // Sprint 32.11: a delete-mutation failure must be visible/announced inside this dialog itself
    // - it stays open on failure (the caller never clears IsOpen from a catch block), and
    // DialogFocusScope keeps keyboard/screen-reader focus trapped here regardless.
    [Fact]
    public void ErrorMessage_RendersAsAnAlertInsideTheDialog()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete transaction")
            .Add(component => component.Message, "Are you sure?")
            .Add(component => component.ErrorMessage, "Could not delete the transaction."));

        var alert = cut.Find(".delete-confirmation__error");
        Assert.Equal("alert", alert.GetAttribute("role"));
        Assert.Contains("Could not delete the transaction.", alert.TextContent);
    }

    [Fact]
    public void WhenErrorMessageIsNotSet_RendersNoErrorAlert()
    {
        using var context = CreateContext();
        var cut = context.Render<BeeDayConfirmDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete transaction")
            .Add(component => component.Message, "Are you sure?"));

        Assert.Empty(cut.FindAll(".delete-confirmation__error"));
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext().WithLocalization();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }
}
