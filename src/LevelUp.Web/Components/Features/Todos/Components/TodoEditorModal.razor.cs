using LevelUp.Web.Components.Features.Todos.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace LevelUp.Web.Components.Features.Todos.Components;

public partial class TodoEditorModal
{
    [Parameter, EditorRequired] public TodoEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<TodoEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
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
}
