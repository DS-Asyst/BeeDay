using BeeDay.Web.Components.DesignSystem.Buttons;
using BeeDay.Web.Components.DesignSystem.Icons;

namespace BeeDay.Web.Tests.Components.Buttons;

public sealed class BeeDayButtonTests
{
    [Fact]
    public void RendersChildContentAndDefaultType()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .AddChildContent("SAVE"));

        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Contains("SAVE", button.TextContent);
        Assert.Contains("beeday-button--primary", button.ClassList);
    }

    [Theory]
    [InlineData(BeeDayButtonVariant.Primary, "beeday-button--primary")]
    [InlineData(BeeDayButtonVariant.Secondary, "beeday-button--secondary")]
    [InlineData(BeeDayButtonVariant.Success, "beeday-button--success")]
    [InlineData(BeeDayButtonVariant.Warning, "beeday-button--warning")]
    [InlineData(BeeDayButtonVariant.Back, "beeday-button--back")]
    [InlineData(BeeDayButtonVariant.Danger, "beeday-button--danger")]
    [InlineData(BeeDayButtonVariant.ConfirmationDanger, "beeday-button--confirmation-danger")]
    [InlineData(BeeDayButtonVariant.ConfirmationCancel, "beeday-button--confirmation-cancel")]
    public void AppliesVariantClass(BeeDayButtonVariant variant, string expectedClass)
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Variant, variant));

        Assert.Contains(expectedClass, cut.Find("button").ClassList);
    }

    [Fact]
    public void AppliesDisabledAndLoadingState()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.IsLoading, true));

        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Equal("true", button.GetAttribute("aria-busy"));
        Assert.Contains("beeday-button--loading", button.ClassList);
        Assert.NotNull(cut.Find("svg.beeday-icon--loading.beeday-button__loader"));
        Assert.Equal("true", cut.Find(".beeday-button__label").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void AppliesLayoutModifiersAndLoadingDisablesButton()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.FullWidth, true)
            .Add(component => component.Compact, true)
            .Add(component => component.IsLoading, true));

        var button = cut.Find("button");
        Assert.Contains("beeday-button--full-width", button.ClassList);
        Assert.Contains("beeday-button--compact", button.ClassList);
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void RendersBeeDayIconWhenConfigured()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Icon, BeeDayIconName.Save)
            .AddChildContent("SAVE"));

        var icon = cut.Find("svg.beeday-icon--save");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Contains("SAVE", cut.Find(".beeday-button__label").TextContent);
    }

    [Fact]
    public void InvokesClickCallback()
    {
        using var context = new BunitContext();
        var clicked = false;
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.True(clicked);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void DoesNotInvokeClickWhenUnavailable(bool disabled, bool loading)
    {
        using var context = new BunitContext();
        var clicks = 0;
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Disabled, disabled)
            .Add(component => component.IsLoading, loading)
            .Add(component => component.OnClick, () => clicks++));

        cut.Find("button").Click();

        Assert.Equal(0, clicks);
    }

    [Fact]
    public void PreservesExplicitType()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Type, "submit"));

        Assert.Equal("submit", cut.Find("button").GetAttribute("type"));
    }

    [Fact]
    public void MergesCustomClassAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Class, "custom-action")
            .AddUnmatched("data-testid", "save-button"));

        var button = cut.Find("button");
        Assert.Contains("custom-action", button.ClassList);
        Assert.Equal("save-button", button.GetAttribute("data-testid"));
    }

    [Fact]
    public void LoadingPreservesLabelContentToKeepTheIntrinsicWidthStable()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.IsLoading, true)
            .AddChildContent("Save changes"));

        Assert.Equal("Save changes", cut.Find(".beeday-button__label").TextContent);
        Assert.Equal("true", cut.Find("button").GetAttribute("aria-busy"));
    }
}
