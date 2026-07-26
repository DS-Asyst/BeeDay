using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace LevelUp.Web.Components.DesignSystem.Modals;

public partial class EditorModalShell
{
    [Parameter, EditorRequired] public object Model { get; set; } = default!;
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string TitleId { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string SubmitLabel { get; set; } = "SAVE";
    [Parameter] public bool ShowDelete { get; set; }
    [Parameter] public bool IsBusy { get; set; }
    [Parameter] public RenderFragment? HeroContent { get; set; }
    [Parameter] public RenderFragment? BodyContent { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnDelete { get; set; }

    private Task Submit(EditContext _) => IsBusy ? Task.CompletedTask : OnSubmit.InvokeAsync();

    private Task Cancel() => IsBusy ? Task.CompletedTask : OnCancel.InvokeAsync();

    private Task HandleKeyDown(KeyboardEventArgs args)
        => args.Key == "Escape" ? Cancel() : Task.CompletedTask;
}
