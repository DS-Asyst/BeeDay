using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LevelUp.UI.Layout;

public static class StatisticRow
{
    public static IRenderable[] BuildText(
        string label,
        string value,
        string? valueStyle = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(value);

        string style = valueStyle ?? LevelUpTheme.Text;

        return Build(
            label,
            new Markup(
                $"[{style}]{Markup.Escape(value)}[/]"
            )
        );
    }

    public static IRenderable[] BuildMarkup(
        string label,
        string markup
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markup);

        return Build(
            label,
            new Markup(markup)
        );
    }

    public static IRenderable[] Build(
        string label,
        IRenderable value
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(value);

        return
        [
            new Markup(
                $"[bold {LevelUpTheme.MutedText}]" +
                $"{Markup.Escape(label)}[/]"
            ),
            value
        ];
    }

    public static IRenderable[] Build(
        string label,
        string value,
        string? valueStyle = null
    )
    {
        return BuildText(label, value, valueStyle);
    }
}
