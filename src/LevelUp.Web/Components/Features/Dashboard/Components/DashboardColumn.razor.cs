using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class DashboardColumn
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string EmptyLabel { get; set; } = "items";
    [Parameter] public int Count { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    private string HeadingId => $"dashboard-{Title.ToLowerInvariant().Replace(" ", "-")}";

}
