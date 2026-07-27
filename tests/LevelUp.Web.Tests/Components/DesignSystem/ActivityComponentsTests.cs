using LevelUp.Web.Components.DesignSystem.Activities;
using LevelUp.Web.Components.Features.Dashboard.Components;

namespace LevelUp.Web.Tests.Components.DesignSystem;

public sealed class ActivityComponentsTests
{
    [Fact]
    public void ActivityCard_RendersSharedVisualContract()
    {
        using var context = new BunitContext();
        var cut = context.Render<ActivityCard>(parameters => parameters
            .Add(component => component.Title, "Read chapter")
            .Add(component => component.Description, "Architecture notes")
            .Add(component => component.Variant, "task"));

        Assert.Contains("activity-card--task", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Read chapter", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyState_DelegatesToDesignSystemImplementation()
    {
        using var context = new BunitContext();
        var cut = context.Render<EmptyState>(parameters => parameters
            .Add(component => component.Title, "Nothing here")
            .Add(component => component.Description, "Create the first item."));

        Assert.Contains("Nothing here", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Create the first item.", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ConfirmationDialog_UsesUnifiedDialogContract()
    {
        using var context = new BunitContext();
        var cut = context.Render<ConfirmationDialog>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Title, "Delete activity")
            .Add(component => component.Message, "Confirm deletion."));

        Assert.NotNull(cut.Find("[role='alertdialog']"));
    }
}
