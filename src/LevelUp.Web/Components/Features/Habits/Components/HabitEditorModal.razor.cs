using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Habits.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LevelUp.Web.Components.Features.Habits.Components;

public partial class HabitEditorModal
{
    [Parameter, EditorRequired] public HabitEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<HabitEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    private bool showDeleteConfirmation;
    private bool AllowsPositive => Model.Direction is HabitDirection.Positive or HabitDirection.Both;
    private bool AllowsNegative => Model.Direction is HabitDirection.Negative or HabitDirection.Both;
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
