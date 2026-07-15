using LevelUp.Domain.Attributes;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Character;

public sealed class AttributeTable
{
    private readonly PlayerAttributes _attributes;

    public AttributeTable(PlayerAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        _attributes = attributes;
    }

    public Table Build()
    {
        Table table = new()
        {
            Border = TableBorder.Rounded,
            Title = new TableTitle(
                $"[bold {LevelUpTheme.Primary}]Attributes[/]"
            )
        };

        table.AddColumn(
            new TableColumn("[bold]Attribute[/]")
        );

        table.AddColumn(
            new TableColumn("[bold]Level[/]").Centered()
        );

        table.AddRow(
            $"{UIIcons.Strength} Strength",
            FormatLevel(_attributes.Strength.Level)
        );

        table.AddRow(
            $"{UIIcons.Intelligence} Intelligence",
            FormatLevel(_attributes.Intelligence.Level)
        );

        table.AddRow(
            $"{UIIcons.Vitality} Vitality",
            FormatLevel(_attributes.Vitality.Level)
        );

        table.AddRow(
            $"{UIIcons.Agility} Agility",
            FormatLevel(_attributes.Agility.Level)
        );

        table.AddRow(
            $"{UIIcons.Dexterity} Dexterity",
            FormatLevel(_attributes.Dexterity.Level)
        );

        table.AddRow(
            $"{UIIcons.Luck} Luck",
            FormatLevel(_attributes.Luck.Level)
        );

        table.Expand();

        return table;
    }

    private static string FormatLevel(int level)
    {
        return $"[bold {LevelUpTheme.Accent}]{level}[/]";
    }
}
