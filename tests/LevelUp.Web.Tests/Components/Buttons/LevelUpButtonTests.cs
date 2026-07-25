using LevelUp.Web.Components.DesignSystem.Buttons;
using LevelUp.Web.Components.DesignSystem.Icons;

namespace LevelUp.Web.Tests.Components.Buttons;

public sealed class LevelUpButtonTests
{
    [Fact]
    public void RendersChildContentAndDefaultType()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .AddChildContent("SAVE"));

        var button = cut.Find("button");
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.Contains("SAVE", button.TextContent);
        Assert.Contains("levelup-button--primary", button.ClassList);
    }

    [Theory]
    [InlineData(LevelUpButtonVariant.Primary, "levelup-button--primary")]
    [InlineData(LevelUpButtonVariant.Secondary, "levelup-button--secondary")]
    [InlineData(LevelUpButtonVariant.Danger, "levelup-button--danger")]
    [InlineData(LevelUpButtonVariant.ConfirmationDanger, "levelup-button--confirmation-danger")]
    [InlineData(LevelUpButtonVariant.ConfirmationCancel, "levelup-button--confirmation-cancel")]
    public void AppliesVariantClass(LevelUpButtonVariant variant, string expectedClass)
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .Add(component => component.Variant, variant));

        Assert.Contains(expectedClass, cut.Find("button").ClassList);
    }

    [Fact]
    public void AppliesDisabledAndLoadingState()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .Add(component => component.Disabled, true)
            .Add(component => component.IsLoading, true));

        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Equal("true", button.GetAttribute("aria-busy"));
        Assert.NotNull(cut.Find(".levelup-button__loader"));
    }

    [Fact]
    public void AppliesLayoutModifiersAndLoadingDisablesButton()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpButton>(parameters => parameters
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
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .Add(component => component.Icon, LevelUpIconName.Save)
            .AddChildContent("SAVE"));

        var icon = cut.Find("svg.levelup-icon--save");
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
        Assert.Contains("SAVE", cut.Find(".levelup-button__label").TextContent);
    }

    [Fact]
    public void InvokesClickCallback()
    {
        using var context = new BunitContext();
        var clicked = false;
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .Add(component => component.OnClick, () => clicked = true));

        cut.Find("button").Click();

        Assert.True(clicked);
    }

    [Fact]
    public void MergesCustomClassAndAdditionalAttributes()
    {
        using var context = new BunitContext();
        var cut = context.Render<LevelUpButton>(parameters => parameters
            .Add(component => component.Class, "custom-action")
            .AddUnmatched("data-testid", "save-button"));

        var button = cut.Find("button");
        Assert.Contains("custom-action", button.ClassList);
        Assert.Equal("save-button", button.GetAttribute("data-testid"));
    }
}
