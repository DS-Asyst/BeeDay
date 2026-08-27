using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Habits.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Habits.Components;

public partial class HabitEditorModal
{
    [Inject] private IStringLocalizer<HabitResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public HabitEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<HabitEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }
    private bool showDeleteConfirmation;
    private string VisualStateClass => HabitVisualState.GetEditorClass(Model.VisualBalance);
    private bool AllowsPositive => Model.Direction is HabitDirection.Positive or HabitDirection.Both;
    private bool AllowsNegative => Model.Direction is HabitDirection.Negative or HabitDirection.Both;
    private string PositiveAriaPressed => AllowsPositive ? "true" : "false";
    private string NegativeAriaPressed => AllowsNegative ? "true" : "false";

    private string FormatDifficulty(HabitDifficulty difficulty) => difficulty switch
    {
        HabitDifficulty.Trivial => Localizer["DifficultyTrivial"],
        HabitDifficulty.Easy => Localizer["DifficultyEasy"],
        HabitDifficulty.Medium => Localizer["DifficultyMedium"],
        HabitDifficulty.Hard => Localizer["DifficultyHard"],
        _ => difficulty.ToString()
    };

    private string FormatResetCounter(HabitResetCounter resetCounter) => resetCounter switch
    {
        HabitResetCounter.Daily => Localizer["ResetCounterDaily"],
        HabitResetCounter.Weekly => Localizer["ResetCounterWeekly"],
        HabitResetCounter.Monthly => Localizer["ResetCounterMonthly"],
        _ => resetCounter.ToString()
    };
    private void TogglePositive() => Model.Direction = (AllowsPositive, AllowsNegative) switch { (true, true) => HabitDirection.Negative, (true, false) => HabitDirection.Positive, _ => HabitDirection.Both };
    private void ToggleNegative() => Model.Direction = (AllowsPositive, AllowsNegative) switch { (true, true) => HabitDirection.Positive, (false, true) => HabitDirection.Negative, _ => HabitDirection.Both };
    private Task Save() => OnSave.InvokeAsync(Model);
    private Task Cancel() { showDeleteConfirmation = false; return OnCancel.InvokeAsync(); }
    private void RequestDelete() => showDeleteConfirmation = true;
    private void CloseDeleteConfirmation() => showDeleteConfirmation = false;
    private async Task ConfirmDelete() { showDeleteConfirmation = false; await OnDelete.InvokeAsync(); }
    private Task HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key != "Escape")
        {
            return Task.CompletedTask;
        }

        if (showDeleteConfirmation)
        {
            showDeleteConfirmation = false;
            return Task.CompletedTask;
        }

        return Cancel();
    }
}
