using BeeDay.Web.Components.Features.Common;
using Microsoft.AspNetCore.Components;

namespace BeeDay.Web.Components.Features.Dashboard.Components;

public partial class ActivityFilterBar : IDisposable
{
    private const int SearchDebounceMilliseconds = 300;
    private bool showCreateMenu;
    private string inputValue = string.Empty;
    private CancellationTokenSource? debounceCancellation;

    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<ActivityType> OnCreate { get; set; }

    protected override void OnParametersSet()
    {
        if (!string.Equals(Value, inputValue, StringComparison.Ordinal))
        {
            inputValue = Value;
        }
    }

    private async Task OnInput(ChangeEventArgs args)
    {
        inputValue = args.Value?.ToString() ?? string.Empty;
        debounceCancellation?.Cancel();
        debounceCancellation?.Dispose();
        debounceCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(SearchDebounceMilliseconds, debounceCancellation.Token);
            await ValueChanged.InvokeAsync(inputValue);
        }
        catch (OperationCanceledException)
        {
            // A newer input superseded this search.
        }
    }

    private void ToggleCreateMenu() => showCreateMenu = !showCreateMenu;

    private Task CreateHabitAsync() => SelectCreateTypeAsync(ActivityType.Habit);
    private Task CreateTaskAsync() => SelectCreateTypeAsync(ActivityType.Task);
    private Task CreateTodoAsync() => SelectCreateTypeAsync(ActivityType.Todo);
    private Task CreateProjectAsync() => SelectCreateTypeAsync(ActivityType.Project);

    private async Task SelectCreateTypeAsync(ActivityType type)
    {
        showCreateMenu = false;
        await OnCreate.InvokeAsync(type);
    }

    public void Dispose()
    {
        debounceCancellation?.Cancel();
        debounceCancellation?.Dispose();
    }
}
