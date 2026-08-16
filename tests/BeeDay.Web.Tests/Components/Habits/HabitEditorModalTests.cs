using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Habits.Components;
using BeeDay.Web.Components.Features.Habits.Models;
using BeeDay.Web.Tests.Localization;

namespace BeeDay.Web.Tests.Components.Habits;

public sealed class HabitEditorModalTests : BunitContext
{
    public HabitEditorModalTests()
    {
        Services.AddLogging();
        Services.AddLocalization();
    }

    [Theory]
    [InlineData("en-US", true, "Edit Habit", "Save")]
    [InlineData("en-US", false, "Create Habit", "Create")]
    [InlineData("pt-BR", true, "Editar hábito", "Salvar")]
    [InlineData("pt-BR", false, "Criar hábito", "Criar")]
    public void TitleAndSubmitLabel_FollowCultureAndEditingState(string culture, bool isEditing, string expectedTitle, string expectedSubmit)
    {
        var cut = BunitLocalizationSupport.WithUiCulture(culture, () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, isEditing)));

        Assert.Contains(expectedTitle, cut.Markup, StringComparison.Ordinal);
        Assert.Equal(expectedSubmit, cut.Find(".editor-modal__header-save").TextContent.Trim());
    }

    [Fact]
    public void UnderPortugueseUiCulture_FieldLabelsAndDirectionAreLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, false)));

        Assert.Contains("Título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Adicionar um título", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Notas", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Direção do hábito", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Positivo", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Negativo", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void AllDifficultyValues_RenderWithEnglishLabels_UnderEnglishCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#habit-difficulty").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<HabitDifficulty>().Length, options.Count);
        Assert.Equal(["Trivial", "Easy", "Medium", "Hard"], options);
    }

    [Fact]
    public void AllDifficultyValues_RenderWithPortugueseLabels_UnderPortugueseCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#habit-difficulty").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<HabitDifficulty>().Length, options.Count);
        Assert.Equal(["Trivial", "Fácil", "Médio", "Difícil"], options);
    }

    [Fact]
    public void AllResetCounterValues_RenderWithEnglishLabels_UnderEnglishCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("en-US", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#habit-reset-counter").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<HabitResetCounter>().Length, options.Count);
        Assert.Equal(["Daily", "Weekly", "Monthly"], options);
    }

    [Fact]
    public void AllResetCounterValues_RenderWithPortugueseLabels_UnderPortugueseCulture()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel())
            .Add(component => component.IsEditing, false)));

        var options = cut.Find("#habit-reset-counter").Children.Select(option => option.TextContent).ToList();

        Assert.Equal(Enum.GetValues<HabitResetCounter>().Length, options.Count);
        Assert.Equal(["Diariamente", "Semanalmente", "Mensalmente"], options);
    }

    [Fact]
    public void HabitTitleAndNotes_TypedByTheUser_AreNeverLocalized()
    {
        var cut = BunitLocalizationSupport.WithUiCulture("pt-BR", () => Render<HabitEditorModal>(parameters => parameters
            .Add(component => component.Model, new HabitEditorModel { Title = "Beber água", Description = "Pelo menos 2 litros por dia" })
            .Add(component => component.IsEditing, true)));

        Assert.Contains("Beber água", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("Pelo menos 2 litros por dia", cut.Markup, StringComparison.Ordinal);
    }

    [Fact]
    public void UnderPortugueseUiCulture_DeleteConfirmationIsLocalized()
    {
        BunitLocalizationSupport.WithUiCulture("pt-BR", () =>
        {
            var cut = Render<HabitEditorModal>(parameters => parameters
                .Add(component => component.Model, new HabitEditorModel { Title = "Beber água" })
                .Add(component => component.IsEditing, true));

            cut.Find(".editor-modal__footer-danger .beeday-button--danger").Click();

            var dialog = cut.Find("[role='alertdialog']");
            Assert.Contains("Excluir hábito", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Tem certeza de que deseja excluir este hábito?", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Esta ação não pode ser desfeita.", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Todas as informações serão removidas permanentemente.", dialog.TextContent, StringComparison.Ordinal);
            Assert.Contains("Cancelar", dialog.TextContent, StringComparison.Ordinal);
        });
    }
}
