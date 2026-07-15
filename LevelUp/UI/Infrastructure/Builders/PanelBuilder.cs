using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LevelUp.UI.Infrastructure.Builders;

public static class PanelBuilder
{
    public static Panel Build(
        string title,
        IRenderable content,
        string? icon = null,
        bool expand = false
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);

        string headerText = string.IsNullOrWhiteSpace(icon)
            ? Markup.Escape(title)
            : $"{Markup.Escape(icon)} {Markup.Escape(title)}";

        Panel panel = new(content)
        {
            Header = new PanelHeader(
                $"[bold {LevelUpTheme.Primary}]{headerText}[/]"
            ),
            Border = BoxBorder.Rounded,
            Padding = new Padding(1, 1),
            Expand = expand
        };

        return panel;
    }
}
