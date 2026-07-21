using LevelUp.Web.Components.DesignSystem.Buttons;

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
        Assert.Contains("editor-modal__save", button.ClassList);
    }

    [Theory]
    [InlineData(LevelUpButtonVariant.Primary, "editor-modal__save")]
    [InlineData(LevelUpButtonVariant.Secondary, "editor-modal__cancel")]
    [InlineData(LevelUpButtonVariant.Danger, "editor-modal__delete")]
    [InlineData(LevelUpButtonVariant.ConfirmationDanger, "delete-confirmation__delete-button")]
    [InlineData(LevelUpButtonVariant.ConfirmationCancel, "delete-confirmation__cancel-button")]
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
