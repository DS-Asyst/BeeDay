using BeeDay.Web.Components.DesignSystem.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Dashboard.Components;

/// <summary>
/// The strings that vary per grammatical state (active/completed) or gender (Portuguese noun
/// agreement differs by category — "hábitos ativos" vs. "tarefas ativas") are supplied fully
/// composed by the caller via DashboardResources, rather than assembled here from fragments —
/// composing "No completed {0}" generically would produce grammatically wrong Portuguese for
/// feminine categories like Tasks/To-Dos.
/// </summary>
public partial class DashboardColumn
{
    [Inject] private IStringLocalizer<DashboardResources> Localizer { get; set; } = default!;

    private bool showCompleted;

    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyTitle { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyDescription { get; set; } = string.Empty;
    [Parameter] public BeeDayIconName EmptyIcon { get; set; } = BeeDayIconName.Information;
    [Parameter] public string? SingularLabel { get; set; }
    [Parameter] public int ActiveCount { get; set; }
    [Parameter] public int CompletedCount { get; set; }
    [Parameter] public bool ShowCompletedSection { get; set; } = true;
    [Parameter] public bool ShowCreateButton { get; set; } = true;
    [Parameter] public string? ActiveStateLabel { get; set; }
    [Parameter] public string? CompletedStateLabel { get; set; }
    [Parameter] public string? ShowActiveAriaLabel { get; set; }
    [Parameter] public string? ShowCompletedAriaLabel { get; set; }
    [Parameter] public string? CompletedEmptyTitle { get; set; }
    [Parameter] public string? CompletedEmptyDescription { get; set; }
    [Parameter] public EventCallback OnCreate { get; set; }
    [Parameter] public bool ShowClearFilterAction { get; set; }
    [Parameter] public EventCallback OnClearFilter { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? ActiveContent { get; set; }
    [Parameter] public RenderFragment? CompletedContent { get; set; }

    private string NormalizedTitle => Title.ToLowerInvariant().Replace(" ", "-");
    private string HeadingId => $"dashboard-{NormalizedTitle}";
    private string ResolvedSingularLabel => string.IsNullOrWhiteSpace(SingularLabel) ? Title.TrimEnd('s') : SingularLabel;
    private BeeDayIconName CurrentViewIcon => showCompleted ? BeeDayIconName.Completed : BeeDayIconName.Repeat;
    private int CurrentCount => showCompleted && ShowCompletedSection ? CompletedCount : ActiveCount;
    private string CurrentCountLabel => $"{CurrentCount} {(showCompleted ? CompletedStateLabel : ActiveStateLabel)}";
    private string AriaPressed => showCompleted ? "true" : "false";
    private string ToggleViewLabel => showCompleted ? ShowActiveAriaLabel ?? string.Empty : ShowCompletedAriaLabel ?? string.Empty;
    private string AddItemAriaLabel => Localizer["DashboardAddItemAriaLabel", ResolvedSingularLabel];

    private void ToggleCompleted() => showCompleted = !showCompleted;
}
