using BeeDay.Web.Components.Features.Projects.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BeeDay.Web.Components.Features.Projects.Components;

public partial class ProjectEditorModal
{
    [Parameter, EditorRequired] public ProjectEditorModel Model { get; set; } = new();
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public EventCallback<ProjectEditorModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }
    [Parameter] public EventCallback OnOpenProject { get; set; }
    private bool showDeleteConfirmation;
    private Task Save() => OnSave.InvokeAsync(Model);
    private Task Cancel() { showDeleteConfirmation = false; return OnCancel.InvokeAsync(); }
    private Task OpenProject() => OnOpenProject.InvokeAsync();
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
    private static string FormatEnum(string value) => System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
}
