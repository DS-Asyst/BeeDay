using LevelUp.Domain.Habits;
using CharacterModel = LevelUp.Domain.Character.Character;
using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using LevelUp.UI.Layout;
using Spectre.Console;

namespace LevelUp.UI.Components.Training;

public sealed class TrainingResultCard
{
    private readonly Habit _habit;
    private readonly CharacterModel _character;
    private readonly decimal _experienceEarned;

    public TrainingResultCard(
        Habit habit,
        CharacterModel character,
        decimal experienceEarned
    )
    {
        ArgumentNullException.ThrowIfNull(habit);
        ArgumentNullException.ThrowIfNull(character);

        _habit = habit;
        _character = character;
        _experienceEarned = experienceEarned;
    }

    public Panel Build()
    {
        Grid summary = new();

        summary.AddColumn(
            new GridColumn().NoWrap()
        );

        summary.AddColumn();

        summary.AddRow(
            StatisticRow.Build(
                "Treinamentos",
                _habit.Title,
                $"bold {LevelUpTheme.Text}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experiência obtida",
                $"+{_experienceEarned:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Atributo",
                _habit.AttributeType.ToString(),
                LevelUpTheme.Accent
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experiência do atributo",
                $"+{_habit.AttributeExperienceReward:0.##} XP",
                $"bold {LevelUpTheme.Success}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Conclusões",
                _habit.TimesCompleted.ToString()
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Nível do personagem",
                _character.Level.ToString(),
                $"bold {LevelUpTheme.Primary}"
            )
        );

        summary.AddRow(
            StatisticRow.Build(
                "Experiência do personagem",
                $"{_character.Experience:0.##}/" +
                $"{_character.ExperienceToNextLevel:0.##}"
            )
        );

        return PanelBuilder.Build(
            title: "Treinamento concluído",
            content: summary,
            icon: UIIcons.Success,
            expand: false
        );
    }
}
