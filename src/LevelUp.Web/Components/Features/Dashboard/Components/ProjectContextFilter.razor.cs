using LevelUp.Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class ProjectContextFilter
{
    [Parameter] public IReadOnlyList<Project> Projects { get; set; } = [];
    [Parameter] public Guid? SelectedProjectId { get; set; }
    [Parameter] public EventCallback<Guid?> SelectedProjectIdChanged { get; set; }

    private string SelectId => "dashboard-project-context";
    private string SelectedValue => SelectedProjectId?.ToString() ?? string.Empty;

    private Task HandleSelectionChanged(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        Guid? projectId = Guid.TryParse(value, out var parsedProjectId) ? parsedProjectId : null;
        return SelectedProjectIdChanged.InvokeAsync(projectId);
    }
}
