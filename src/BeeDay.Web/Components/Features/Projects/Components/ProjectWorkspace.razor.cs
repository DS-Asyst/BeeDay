using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Projects.Components;

public partial class ProjectWorkspace
{
    [Inject] private IStringLocalizer<ProjectResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public ProjectSummary? Project { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoSummary> OnEditTodo { get; set; }
    [Parameter] public EventCallback<TodoSummary> OnDeleteTodo { get; set; }

    private bool showTodos = true;

    private string StatusLabel => Project?.Status switch
    {
        ProjectStatus.InProgress => Localizer["StatusInProgress"],
        ProjectStatus.Completed => Localizer["StatusCompleted"],
        _ => Localizer["StatusPlanned"]
    };

    private void ToggleTodos()
    {
        showTodos = !showTodos;
    }
}
