using LevelUp.UI.Infrastructure;
using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using LevelUp.UI.Layout;
using Spectre.Console;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.UI.Components.Character;

public sealed class CharacterCard
{
    private const int ExperienceBarWidth = 30;
    private readonly CharacterModel character;

    public CharacterCard(CharacterModel character)
    {
        ArgumentNullException.ThrowIfNull(character);
        this.character = character;
    }

    public Panel Build()
    {
        Grid summary = new();
        summary.AddColumn(new GridColumn().NoWrap());
        summary.AddColumn();
        summary.AddRow(StatisticRow.Build("Name", character.Name, $"bold {LevelUpTheme.Text}"));
        summary.AddRow(StatisticRow.Build("Classe", DisplayText.For(character.Class), LevelUpTheme.Accent));
        summary.AddRow(StatisticRow.Build("Title", DisplayText.For(character.Rank), LevelUpTheme.Gold));
        summary.AddRow(StatisticRow.Build("Level", character.Level.ToString(), $"bold {LevelUpTheme.Primary}"));
        summary.AddRow(StatisticRow.Build("Experience", $"{character.Experience}/{character.ExperienceToNextLevel}"));
        summary.AddRow(
            new Markup($"[bold {LevelUpTheme.MutedText}]Progress[/]"),
            BuildExperienceBar()
        );

        return PanelBuilder.Build(
            title: "Character",
            content: summary,
            icon: UIIcons.Character,
            expand: false
        );
    }

    private Markup BuildExperienceBar()
    {
        decimal percentage = character.ExperienceToNextLevel <= 0
            ? 0m
            : Math.Clamp(character.Experience / character.ExperienceToNextLevel * 100m, 0m, 100m);
        int completedBlocks = Math.Clamp(
            (int)Math.Round(percentage / 100m * ExperienceBarWidth),
            0,
            ExperienceBarWidth
        );
        string completed = new('█', completedBlocks);
        string remaining = new('░', ExperienceBarWidth - completedBlocks);
        return new Markup(
            $"[{LevelUpTheme.Experience}]{completed}[/]" +
            $"[{LevelUpTheme.MutedText}]{remaining}[/] " +
            $"[{LevelUpTheme.Primary}]{percentage:0}%[/]"
        );
    }
}
