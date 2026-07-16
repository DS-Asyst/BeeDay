using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Shared;

public sealed class ComingSoonCard
{
    private readonly string _featureName;

    public ComingSoonCard(string featureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        _featureName = featureName;
    }

    public Panel Build()
    {
        var content = new Markup(
            $"[bold {LevelUpTheme.Text}]{Markup.Escape(_featureName)}[/]\n\n" +
            $"[{LevelUpTheme.MutedText}]This feature is currently under development and will be available in a future phase of the project.[/]"
        );

        return PanelBuilder.Build(
            title: "Em breve",
            content: content,
            icon: UIIcons.Information,
            expand: true
        );
    }
}
