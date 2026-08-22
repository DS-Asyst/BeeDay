using BeeDay.Web.Components.Features.Projects.Components;
using BeeDay.Web.Components.Features.Projects.Models;
using BeeDay.Web.Tests.Localization;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Tests.Components.Projects;

public sealed class ProjectEditorModalTests : BunitContext
{
    public ProjectEditorModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void OpenProjectAction_RendersOnlyWhenEditing()
    {
        var editingCut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true)));

        Assert.Contains(editingCut.FindAll("button"), button => button.TextContent.Contains("Open Project", StringComparison.Ordinal));

        var creatingCut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel())
            .Add(component => component.IsEditing, false)));

        Assert.DoesNotContain(creatingCut.FindAll("button"), button => button.TextContent.Contains("Open Project", StringComparison.Ordinal));
    }

    [Fact]
    public void OpenProjectAction_MatchesTheSharedFooterActionScaleNotACompactVariant()
    {
        // Sprint 29.3: this button previously opted into Compact="true" specifically to match
        // WalletTagManager's smaller "New tag" button — but it sits in the same
        // .editor-modal__footer-actions row as the shell's own Cancel button (EditorModalShell.razor),
        // which is never Compact, so Open Project read as visibly undersized next to it. Compact is
        // removed: Open Project now shares the same height/typography contract as every other footer
        // action across every editor, while its width still varies with the label like any button.
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true)));

        var button = cut.FindAll("button").First(element => element.TextContent.Contains("Open Project", StringComparison.Ordinal));

        Assert.DoesNotContain("beeday-button--compact", button.ClassList);
        Assert.Contains("beeday-button--secondary", button.ClassList);
        Assert.DoesNotContain("beeday-button--comic", button.ClassList);
        Assert.Empty(button.QuerySelectorAll("svg"));
    }

    [Fact]
    public async Task ClickingOpenProject_InvokesOnOpenProject()
    {
        var invoked = false;

        await BunitLocalizationSupport.WithUiCultureAsync("en-US", async () =>
        {
            var cut = Render<ProjectEditorModal>(parameters => parameters
                .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
                .Add(component => component.IsEditing, true)
                .Add(component => component.OnOpenProject, () => invoked = true));

            var button = cut.FindAll("button").First(element => element.TextContent.Contains("Open Project", StringComparison.Ordinal));
            await button.ClickAsync();
        });

        Assert.True(invoked);
    }

    [Theory]
    [InlineData("en-US", true, "Edit Project", "Save")]
    [InlineData("en-US", false, "Create Project", "Create")]
    [InlineData("pt-BR", true, "Editar projeto", "Salvar")]
    [InlineData("pt-BR", false, "Criar projeto", "Criar")]
    public void TitleAndSubmitLabel_FollowCultureAndEditingState(string culture, bool isEditing, string expectedTitle, string expectedSubmit)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel())
            .Add(component => component.IsEditing, isEditing)));

        Assert.Contains(expectedTitle, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(expectedSubmit, cut.Find(".editor-modal__header-save").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_FieldLabelsAndPlaceholdersAreLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel())
            .Add(component => component.IsEditing, false)));

        Assert.Contains("Título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Adicionar um título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Notas", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Data prevista", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Um objetivo maior usado para organizar o trabalho.", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void ProjectTitle_TypedByTheUser_IsNeverLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Reforma da cozinha" })
            .Add(component => component.IsEditing, true)));

        Assert.Contains("Reforma da cozinha", cut.Markup, StringComparison.Ordinal);
    }

    // EPIC 30 Sprint 30.14: mirrors HabitEditorModalTests.Save_PassesTheEditedFieldsToOnSave (Sprint
    // 30.12) and its Task/Todo equivalents (Sprint 30.13) — no prior test proved OnSave actually
    // receives the edited Title/Description/ExpectedDate, only that the callback exists.
    [Fact]
    public async Task Save_PassesTheEditedFieldsToOnSave()
    {
        ProjectEditorModel? saved = null;
        var model = new ProjectEditorModel { Title = "Original", Description = "Original notes" };

        var cut = Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, model)
            .Add(component => component.IsEditing, true)
            .Add(component => component.OnSave, EventCallback.Factory.Create<ProjectEditorModel>(this, m => saved = m)));

        cut.Find("#project-title").Change("Kitchen remodel");
        cut.Find("#project-notes").Change("Cabinets and countertop");
        cut.Find("#project-expected-date").Change("2026-06-15");
        await cut.Find(".editor-modal__header-save").ClickAsync();

        Assert.NotNull(saved);
        Assert.Equal("Kitchen remodel", saved.Title);
        Assert.Equal("Cabinets and countertop", saved.Description);
        Assert.Equal(new DateTime(2026, 6, 15), saved.ExpectedDate);
    }

    // EPIC 30 Sprint 30.14 / BD30-F056: makes the Finding explicit and regression-proof — Archived
    // round-trips correctly at the Domain/Application/persistence layers (EfProjectRepositoryTests),
    // but the rendered form has no control for it at all, so it can never be set to true through the
    // running application. This proves the absence, rather than merely relying on "the razor file
    // has no such element" being true by omission.
    [Fact]
    public void ArchivedField_HasNoRenderedControlToSetIt()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<ProjectEditorModal>(parameters => parameters
            .Add(component => component.Model, new ProjectEditorModel { Title = "Kitchen remodel" })
            .Add(component => component.IsEditing, true)));

        Assert.Empty(cut.FindAll("#project-archived"));
        Assert.Empty(cut.FindAll("input[type='checkbox']"));
        Assert.DoesNotContain("Archived", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnderPortugueseUiCulture_DeleteConfirmationIsLocalized()
    {
        BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
        {
            var cut = Render<ProjectEditorModal>(parameters => parameters
                .Add(component => component.Model, new ProjectEditorModel { Title = "Reforma da cozinha" })
                .Add(component => component.IsEditing, true));

            cut.Find(".editor-modal__footer-danger .beeday-button--danger").Click();

            var dialog = cut.Find("[role='alertdialog']");
            Assert.Contains("Excluir projeto", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Tem certeza de que deseja excluir este projeto?", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Esta ação não pode ser desfeita.", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Todas as informações serão removidas permanentemente.", dialog.TextContent, StringComparison.Ordinal);
            // Cancel reuses the already-localized Design System default, not a Projects-owned override.
            Assert.Contains("Cancelar", dialog.TextContent, StringComparison.Ordinal);
        });
    }
}
