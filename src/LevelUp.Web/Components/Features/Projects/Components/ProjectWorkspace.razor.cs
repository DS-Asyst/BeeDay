using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Projects.Components;

public partial class ProjectWorkspace
{
    [Parameter, EditorRequired] public Project? Project { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback OnAddTodo { get; set; }
    [Parameter] public EventCallback<Todo> OnEditTodo { get; set; }
    [Parameter] public EventCallback<Todo> OnDeleteTodo { get; set; }

    private bool showTodos = true;

    private string StatusLabel => Project?.Status switch
    {
        ProjectStatus.InProgress => "In Progress",
        ProjectStatus.Completed => "Completed",
        _ => "Planned"
    };

    private void ToggleTodos()
    {
        showTodos = !showTodos;
    }
}
