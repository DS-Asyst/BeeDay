using LevelUp.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class DashboardColumn
{
    private bool showCompleted;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyLabel { get; set; } = "items";
    [Parameter, EditorRequired] public string EmptyTitle { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyDescription { get; set; } = string.Empty;
    [Parameter] public PixelIconName EmptyIcon { get; set; } = PixelIconName.Information;
    [Parameter] public string? SingularLabel { get; set; }
    [Parameter] public int ActiveCount { get; set; }
    [Parameter] public int CompletedCount { get; set; }
    [Parameter] public bool ShowCompletedSection { get; set; } = true;
    [Parameter] public bool ShowCreateButton { get; set; } = true;
    [Parameter] public EventCallback OnCreate { get; set; }
    [Parameter] public RenderFragment? ActiveContent { get; set; }
    [Parameter] public RenderFragment? CompletedContent { get; set; }

    private string NormalizedTitle => Title.ToLowerInvariant().Replace(" ", "-");
    private string HeadingId => $"dashboard-{NormalizedTitle}";
    private string ResolvedSingularLabel => string.IsNullOrWhiteSpace(SingularLabel) ? Title.TrimEnd('s') : SingularLabel;
    private string CurrentViewLabel => showCompleted ? "Completed" : "Active";
    private PixelIconName CurrentViewIcon => showCompleted ? PixelIconName.Completed : PixelIconName.Repeat;
    private int CurrentCount => showCompleted && ShowCompletedSection ? CompletedCount : ActiveCount;
    private string CurrentCountLabel => $"{CurrentCount} {CurrentViewLabel.ToLowerInvariant()} {EmptyLabel}";
    private string AriaPressed => showCompleted ? "true" : "false";
    private string ToggleViewLabel => showCompleted
        ? $"Show active {EmptyLabel.ToLowerInvariant()}"
        : $"Show completed {EmptyLabel.ToLowerInvariant()}";
    private string CompletedEmptyTitle => $"No completed {EmptyLabel.ToLowerInvariant()}";
    private string CompletedEmptyDescription => $"Completed {EmptyLabel.ToLowerInvariant()} will appear here.";

    private void ToggleCompleted() => showCompleted = !showCompleted;
}
