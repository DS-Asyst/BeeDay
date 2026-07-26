using LevelUp.Domain.Enums;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class HabitCard
{
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string Description { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;
    [Parameter] public HabitDirection Direction { get; set; } = HabitDirection.Both;
    [Parameter] public ActivityAttribute? Attribute { get; set; }
    [Parameter] public int PositiveCount { get; set; }
    [Parameter] public int NegativeCount { get; set; }
    [Parameter] public bool Featured { get; set; }
    [Parameter] public EventCallback OnPositive { get; set; }
    [Parameter] public EventCallback OnNegative { get; set; }
    [Parameter] public EventCallback OnEdit { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }

    private bool menuOpen;
    private int Balance => PositiveCount - NegativeCount;
    private bool AllowsPositive => Direction is HabitDirection.Positive or HabitDirection.Both;
    private bool AllowsNegative => Direction is HabitDirection.Negative or HabitDirection.Both;

    private void HandleMenuOpenChanged(bool isOpen) => menuOpen = isOpen;

    private string DirectionText => Direction switch
    {
        HabitDirection.Positive => "Positive habit",
        HabitDirection.Negative => "Negative habit",
        _ => "Positive and negative habit"
    };

    private string CardCssClass
    {
        get
        {
            var balanceClass = Balance switch
            {
                >= 21 => "habit-card--sky",
                >= 14 => "habit-card--green",
                >= 7 => "habit-card--yellow",
                <= -14 => "habit-card--red-strong",
                <= -7 => "habit-card--red-medium",
                <= -1 => "habit-card--red-light",
                _ => "habit-card--white"
            };

            return $"habit-card {balanceClass} {(menuOpen ? "habit-card--menu-open" : string.Empty)}";
        }
    }

}
