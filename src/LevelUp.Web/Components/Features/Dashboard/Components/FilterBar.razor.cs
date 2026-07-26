using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Common;
using LevelUp.Web.Components.Features.Dashboard.State;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class FilterBar : IDisposable
{
    private const int SearchDebounceMilliseconds = 300;
    private bool showCreateMenu;
    private string inputValue = string.Empty;
    private CancellationTokenSource? debounceCancellation;

    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public EventCallback<ActivityType> OnCreate { get; set; }
    [Parameter] public ActivityAttribute? Attribute { get; set; }
    [Parameter] public EventCallback<ActivityAttribute?> AttributeChanged { get; set; }
    [Parameter] public ActivitySortOption Sort { get; set; }
    [Parameter] public EventCallback<ActivitySortOption> SortChanged { get; set; }
    [Parameter] public int ResultCount { get; set; }
    [Parameter] public int TotalCount { get; set; }

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

    private string AttributeValue => Attribute?.ToString() ?? string.Empty;

    private Task OnAttributeChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        ActivityAttribute? attribute = Enum.TryParse<ActivityAttribute>(value, out var parsed) ? parsed : null;
        return AttributeChanged.InvokeAsync(attribute);
    }

    private Task OnSortChangedAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString();
        var sort = Enum.TryParse<ActivitySortOption>(value, out var parsed) ? parsed : ActivitySortOption.Manual;
        return SortChanged.InvokeAsync(sort);
    }

    private string ResultSummary => string.IsNullOrWhiteSpace(inputValue)
        ? $"{TotalCount} total"
        : $"{ResultCount} {(ResultCount == 1 ? "result" : "results")}";

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
