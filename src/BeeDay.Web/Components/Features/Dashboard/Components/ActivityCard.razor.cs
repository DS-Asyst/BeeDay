using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDay.Web.Components.Features.Dashboard.Components;

public partial class ActivityCard
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public string Meta { get; set; } = string.Empty;
    [Parameter] public string Variant { get; set; } = "task";
    [Parameter] public bool Featured { get; set; }
    [Parameter] public bool Completed { get; set; }
    [Parameter] public EventCallback OnToggle { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }

    private string CardCssClass =>
        $"activity-card activity-card--{Variant} {(Completed ? "activity-card--completed" : string.Empty)}";

    private string EntityLabel => Variant switch
    {
        "todo" => "To-Do",
        "project" => "Project",
        _ => "Task"
    };

    private Task HandleBodyKeyDown(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? OnEdit.InvokeAsync() : Task.CompletedTask;
}
