using LevelUp.Domain.Attributes;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Character;

public sealed class AttributeTable
{
    private readonly PlayerAttributes attributes;

    public AttributeTable(PlayerAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);
        this.attributes = attributes;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Primary}]Atributos[/]"
            )
        };

        table.AddColumn(new TableColumn("[bold]Atributo[/]"));
        table.AddColumn(new TableColumn("[bold]Level[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Atributo[/]"));
        table.AddColumn(new TableColumn("[bold]Level[/]").Centered());

        table.AddRow(
            $"{UIIcons.Strength} Strength",
            FormatLevel(attributes.Strength.Level),
            $"{UIIcons.Intelligence} Intelligence",
            FormatLevel(attributes.Intelligence.Level)
        );

        table.AddRow(
            $"{UIIcons.Vitality} Vitality",
            FormatLevel(attributes.Vitality.Level),
            $"{UIIcons.Agility} Agility",
            FormatLevel(attributes.Agility.Level)
        );

        table.AddRow(
            $"{UIIcons.Dexterity} Dexterity",
            FormatLevel(attributes.Dexterity.Level),
            $"{UIIcons.Luck} Luck",
            FormatLevel(attributes.Luck.Level)
        );

        return table;
    }

    private static string FormatLevel(int level)
    {
        return $"[bold {LevelUpTheme.Accent}]{level}[/]";
    }
}
