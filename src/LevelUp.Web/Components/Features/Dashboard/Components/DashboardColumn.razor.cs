using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class DashboardColumn
{
    private bool showCompleted;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyLabel { get; set; } = "items";
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
    private string CompletedHeadingId => $"dashboard-{NormalizedTitle}-completed-heading";
    private string CompletedContentId => $"dashboard-{NormalizedTitle}-completed-content";
    private string ResolvedSingularLabel => string.IsNullOrWhiteSpace(SingularLabel) ? Title.TrimEnd('s') : SingularLabel;

    private void ToggleCompleted() => showCompleted = !showCompleted;
}
