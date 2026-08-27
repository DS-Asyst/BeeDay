using BeeDay.Domain.Enums;
using BeeDay.Web.Components.Features.Tasks.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Components.Features.Tasks.Components;

public partial class TaskEditorModal
{
    [Inject] private IStringLocalizer<TaskResources> Localizer { get; set; } = default!;

    [Parameter, EditorRequired] public TaskEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<TaskEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }
    private bool showDeleteConfirmation;
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
    private string FormatRepeat(TaskRepeat repeat) => repeat switch
    {
        TaskRepeat.None => Localizer["RepeatNone"],
        TaskRepeat.Daily => Localizer["RepeatDaily"],
        TaskRepeat.Weekly => Localizer["RepeatWeekly"],
        TaskRepeat.Monthly => Localizer["RepeatMonthly"],
        _ => repeat.ToString()
    };
}
