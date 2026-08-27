using BeeDay.Application.Features.Dashboard.Responses;
using BeeDay.Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Projects.Components;

public partial class ProjectWorkspace
{
    [Inject] private IStringLocalizer<ProjectResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public ProjectSummary? Project { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnAddTodo { get; set; }
    [Parameter] public EventCallback<TodoSummary> OnToggleTodo { get; set; }
    [Parameter] public EventCallback<TodoSummary> OnEditTodo { get; set; }
    [Parameter] public EventCallback<TodoSummary> OnDeleteTodo { get; set; }

    private bool showTodos = true;
    private readonly string id = Guid.NewGuid().ToString("N");
    private string DialogId => $"project-workspace-{id}";
    private string TitleId => $"project-workspace-title-{id}";

    private string StatusLabel => Project?.Status switch
    {
        ProjectStatus.InProgress => Localizer["StatusInProgress"],
        ProjectStatus.Completed => Localizer["StatusCompleted"],
        _ => Localizer["StatusPlanned"]
    };

    private string ToggleTodoAriaLabel(TodoSummary todo) => todo.Completed
        ? Localizer["TodoMarkIncompleteAriaLabel", todo.Title]
        : Localizer["TodoCompleteAriaLabel", todo.Title];

    private void ToggleTodos()
    {
        showTodos = !showTodos;
    }

    private Task HandleKeyDown(KeyboardEventArgs args) =>
        args.Key == "Escape" ? OnClose.InvokeAsync() : Task.CompletedTask;
}
