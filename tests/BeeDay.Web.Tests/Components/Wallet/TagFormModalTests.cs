using BeeDay.Web.Components.Features.Wallets.Components;
using BeeDay.Web.Components.Features.Wallets.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Wallet;

public sealed class TagFormModalTests : BunitContext
{
    public TagFormModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ShowsDeleteOnlyWhenEditing()
    {
        var editingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.NotEmpty(editingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));

        var creatingCut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.Empty(creatingCut.FindAll(".editor-modal__footer-danger .beeday-button--danger"));
    }

    [Fact]
    public async Task ClickingDelete_InvokesOnDeleteRequested_WithoutASecondConfirmationDialog()
    {
        var deleteRequested = false;
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new WalletTagFormModel())
            .Add(component => component.OnDeleteRequested, () => deleteRequested = true));

        await cut.Find(".editor-modal__footer-danger .beeday-button--danger").ClickAsync();

        Assert.True(deleteRequested);
        Assert.Empty(cut.FindAll(".delete-confirmation"));
    }

    [Theory]
    [InlineData("en-US", "Use a valid hexadecimal color.")]
    [InlineData("pt-BR", "Use uma cor hexadecimal válida.")]
    public async Task InvalidColor_ShowsALocalizedValidationMessage(string culture, string expected)
    {
        await BunitLocalizationSupport.WithUiCultureAsync(culture, async () =>
        {
            var cut = Render<TagFormModal>(parameters => parameters
                .Add(component => component.IsOpen, true)
                .Add(component => component.IsEditing, false)
                .Add(component => component.Model, new WalletTagFormModel { Name = "Groceries", Color = "not-a-color" }));

            await cut.Find(".editor-modal__header-save").ClickAsync();

            cut.WaitForAssertion(() => Assert.Contains(expected, cut.Markup, StringComparison.Ordinal));
        });
    }

    [Fact]
    public void RendersAsASiblingShapeMatchingTheSharedEditorModalShell()
    {
        // Uses the exact same EditorModalShell markup contract as every other editor modal
        // (Transaction, Project, Habit, Task, To-Do) — .editor-modal-backdrop / .editor-modal —
        // with no Wallet-specific modal classes.
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, false)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.NotEmpty(cut.FindAll(".editor-modal-backdrop"));
        Assert.NotEmpty(cut.FindAll(".editor-modal"));
        Assert.Empty(cut.FindAll(".wallet-modal"));
        Assert.Empty(cut.FindAll(".wallet-modal-backdrop"));
    }

    [Fact]
    public void KeepsTheNativeColorPickerAsASpecializedDataControl()
    {
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Model, new WalletTagFormModel { Color = "#12AB34" }));

        var colorPicker = cut.Find("input[type='color']");
        Assert.Equal("#12AB34", colorPicker.GetAttribute("value"));
        Assert.False(string.IsNullOrWhiteSpace(colorPicker.GetAttribute("aria-label")));
    }
}
