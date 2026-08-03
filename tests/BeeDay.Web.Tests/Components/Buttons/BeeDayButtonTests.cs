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
        Assert.Contains("levelup-button--primary", button.ClassList);
    }

    [Theory]
    [InlineData(BeeDayButtonVariant.Primary, "levelup-button--primary")]
    [InlineData(BeeDayButtonVariant.Secondary, "levelup-button--secondary")]
    [InlineData(BeeDayButtonVariant.Success, "levelup-button--success")]
    [InlineData(BeeDayButtonVariant.Warning, "levelup-button--warning")]
    [InlineData(BeeDayButtonVariant.Back, "levelup-button--back")]
    [InlineData(BeeDayButtonVariant.Danger, "levelup-button--danger")]
    [InlineData(BeeDayButtonVariant.ConfirmationDanger, "levelup-button--confirmation-danger")]
    [InlineData(BeeDayButtonVariant.ConfirmationCancel, "levelup-button--confirmation-cancel")]
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
        Assert.NotNull(cut.Find("svg.pixel-icon--loading.levelup-button__loader"));
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
        Assert.Contains("levelup-button--full-width", button.ClassList);
        Assert.Contains("levelup-button--compact", button.ClassList);
        Assert.True(button.HasAttribute("disabled"));
    }

    [Fact]
    public void RendersPixelIconWhenConfigured()
    {
        using var context = new BunitContext();
        var cut = context.Render<BeeDayButton>(parameters => parameters
            .Add(component => component.Icon, PixelIconName.Save)
            .AddChildContent("SAVE"));

        var icon = cut.Find("svg.pixel-icon--save");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Contains("SAVE", cut.Find(".levelup-button__label").TextContent);
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
}
