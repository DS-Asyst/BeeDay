using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Habits;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDay.Web.Components.Features.Dashboard.Components;

public partial class HabitCard
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public HabitDirection Direction { get; set; } = HabitDirection.Both;
    [Parameter] public int PositiveCount { get; set; }
    [Parameter] public int NegativeCount { get; set; }
    [Parameter] public bool Featured { get; set; }
    [Parameter] public EventCallback OnPositive { get; set; }
    [Parameter] public EventCallback OnNegative { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }

    private int Balance => PositiveCount - NegativeCount;
    private string FormattedBalance => Balance > 0 ? $"+{Balance}" : Balance.ToString();
    private bool AllowsPositive => Direction is HabitDirection.Positive or HabitDirection.Both;
    private bool AllowsNegative => Direction is HabitDirection.Negative or HabitDirection.Both;

    private string DirectionText => Direction switch
    {
        HabitDirection.Positive => "Positive habit",
        HabitDirection.Negative => "Negative habit",
        _ => "Positive and negative habit"
    };

    private string CardCssClass => $"habit-card {HabitVisualState.GetCardClass(Balance)}";

    private Task HandleBodyKeyDown(KeyboardEventArgs args) =>
        args.Key is "Enter" or " " ? OnEdit.InvokeAsync() : Task.CompletedTask;
}
