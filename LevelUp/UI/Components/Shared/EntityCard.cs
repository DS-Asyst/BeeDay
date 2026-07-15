using LevelUp.UI.Infrastructure.Builders;
using LevelUp.UI.Layout;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace LevelUp.UI.Components.Shared;

public sealed class EntityCard
{
    private readonly string title;
    private readonly string icon;
    private readonly Grid details = new();

    public EntityCard(
        string title,
        string icon
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(icon);

        this.title = title;
        this.icon = icon;

        details.AddColumn(
            new GridColumn().NoWrap()
        );

        details.AddColumn();
    }

    public EntityCard AddText(
        string label,
        string value,
        string? valueStyle = null
    )
    {
        details.AddRow(
            StatisticRow.BuildText(
                label,
                value,
                valueStyle
            )
        );

        return this;
    }

    public EntityCard AddMarkup(
        string label,
        string markup
    )
    {
        details.AddRow(
            StatisticRow.BuildMarkup(
                label,
                markup
            )
        );

        return this;
    }

    public EntityCard AddRenderable(
        string label,
        IRenderable value
    )
    {
        details.AddRow(
            StatisticRow.Build(label, value)
        );

        return this;
    }

    public Panel Build(bool expand = true)
    {
        return PanelBuilder.Build(
            title,
            details,
            icon,
            expand
        );
    }
}
