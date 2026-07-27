using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class ActivityCard
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public string Meta { get; set; } = string.Empty;
    [Parameter] public string Variant { get; set; } = "task";
    [Parameter] public ActivityAttribute? Attribute { get; set; }
    [Parameter] public bool Featured { get; set; }
    [Parameter] public bool Completed { get; set; }
    [Parameter] public EventCallback OnToggle { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public EventCallback OnOpen { get; set; }

    private bool menuOpen;

    private string CardCssClass =>
        $"activity-card activity-card--{Variant} {(Completed ? "activity-card--completed" : string.Empty)} {(menuOpen ? "activity-card--menu-open" : string.Empty)}";

    private void HandleMenuOpenChanged(bool isOpen) => menuOpen = isOpen;
}
