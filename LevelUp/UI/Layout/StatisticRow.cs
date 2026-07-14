using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LevelUp.UI.Layout;

public static class StatisticRow
{
    public static IRenderable[] Build(
        string label,
        string value,
        string? valueStyle = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(value);

        string style = valueStyle ?? LevelUpTheme.Text;

        return
        [
            new Markup(
                $"[bold {LevelUpTheme.MutedText}]" +
                $"{Markup.Escape(label)}[/]"
            ),
            new Markup(
                $"[{style}]{Markup.Escape(value)}[/]"
            )
        ];
    }
}