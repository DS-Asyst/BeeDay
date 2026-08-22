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
    [InlineData("en-US", "Choose one of the 10 official colors.")]
    [InlineData("pt-BR", "Escolha uma das 10 cores oficiais.")]
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
    public void OffersExactlyTheTenOfficialPaletteSwatchesAndNoFreeColorInput()
    {
        // EPIC 27 Sprint 27.11: no free HEX/RGB input — only the 10 COR0-COR9 swatches.
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Model, new WalletTagFormModel()));

        Assert.Empty(cut.FindAll("input[type='color']"));
        Assert.Empty(cut.FindAll("input[type='text'].wallet-color-field"));
        var swatches = cut.FindAll(".wallet-color-swatch");
        Assert.Equal(10, swatches.Count);
        Assert.All(swatches, swatch => Assert.False(string.IsNullOrWhiteSpace(swatch.GetAttribute("aria-label"))));
    }

    [Fact]
    public void ClickingASwatchSelectsItAndMarksItWithACheckNotJustColor()
    {
        var model = new WalletTagFormModel();
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.Model, model));

        var swatches = cut.FindAll(".wallet-color-swatch");
        var target = swatches[2];
        target.Click();

        cut.WaitForAssertion(() =>
        {
            var refreshedSwatches = cut.FindAll(".wallet-color-swatch");
            var selected = refreshedSwatches.Single(swatch => swatch.ClassList.Contains("wallet-color-swatch--selected"));
            Assert.Equal("true", selected.GetAttribute("aria-checked"));
            Assert.NotNull(selected.QuerySelector(".beeday-icon"));
            Assert.Equal(1, refreshedSwatches.Count(swatch => swatch.GetAttribute("aria-checked") == "true"));
        });
    }

    [Fact]
    public void APreExistingOutOfPaletteColorLeavesEverySwatchUnselectedInsteadOfGuessing()
    {
        // Sprint 25.11 already declared user-persisted tag colors PRODUCT-SPECIFIC and out of
        // scope for migration — editing such a tag must not silently snap it to a nearby swatch.
        var cut = Render<TagFormModal>(parameters => parameters
            .Add(component => component.IsOpen, true)
            .Add(component => component.IsEditing, true)
            .Add(component => component.Model, new WalletTagFormModel { Color = "#12AB34" }));

        Assert.Empty(cut.FindAll(".wallet-color-swatch--selected"));
    }
}
