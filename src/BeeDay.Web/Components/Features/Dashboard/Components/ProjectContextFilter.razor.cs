using LevelUp.Application.Features.Dashboard.Responses;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class ProjectContextFilter
{
    private bool isOpen;

    [Parameter] public IReadOnlyList<ProjectSummary> Projects { get; set; } = [];
    [Parameter] public Guid? SelectedProjectId { get; set; }
    [Parameter] public EventCallback<Guid?> SelectedProjectIdChanged { get; set; }

    private void ToggleMenu() => isOpen = !isOpen;

    private Task SelectAsync(Guid? projectId)
    {
        isOpen = false;
        return SelectedProjectIdChanged.InvokeAsync(projectId);
    }
}
