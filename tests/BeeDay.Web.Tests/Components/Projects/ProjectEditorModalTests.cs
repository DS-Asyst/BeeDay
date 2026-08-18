using BeeDay.Web.Components.Features.Projects.Components;
using BeeDay.Web.Components.Features.Projects.Models;
using BeeDay.Web.Tests.Localization;

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
