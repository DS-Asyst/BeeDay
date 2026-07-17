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
        table.AddColumn(new TableColumn("[bold]Nível[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Atributo[/]"));
        table.AddColumn(new TableColumn("[bold]Nível[/]").Centered());

        table.AddRow(
            $"{UIIcons.Strength} Força",
            FormatLevel(attributes.Strength.Level),
            $"{UIIcons.Intelligence} Inteligência",
            FormatLevel(attributes.Intelligence.Level)
        );

        table.AddRow(
            $"{UIIcons.Vitality} Vitalidade",
            FormatLevel(attributes.Vitality.Level),
            $"{UIIcons.Agility} Agilidade",
            FormatLevel(attributes.Agility.Level)
        );

        table.AddRow(
            $"{UIIcons.Dexterity} Destreza",
            FormatLevel(attributes.Dexterity.Level),
            $"{UIIcons.Luck} Sorte",
            FormatLevel(attributes.Luck.Level)
        );

        return table;
    }

    private static string FormatLevel(int level)
    {
        return $"[bold {LevelUpTheme.Accent}]{level}[/]";
    }
}
