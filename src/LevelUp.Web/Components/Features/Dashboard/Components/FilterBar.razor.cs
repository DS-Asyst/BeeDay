using LevelUp.Domain.Enums;
using LevelUp.Web.Components.Features.Common;
using Microsoft.AspNetCore.Components;

namespace LevelUp.Web.Components.Features.Dashboard.Components;

public partial class FilterBar : IDisposable
{
    private const int SearchDebounceMilliseconds = 300;
    private static readonly ActivityAttribute[] Attributes = Enum.GetValues<ActivityAttribute>();
    private bool showCreateMenu;
    private bool showTagsMenu;
    private string inputValue = string.Empty;
    private CancellationTokenSource? debounceCancellation;

    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public IReadOnlyCollection<ActivityAttribute> SelectedAttributes { get; set; } = Array.Empty<ActivityAttribute>();
    [Parameter] public EventCallback<ActivityAttribute> OnAttributeToggle { get; set; }
    [Parameter] public EventCallback OnClearAttributes { get; set; }
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

    private void ToggleTagsMenu()
    {
        showTagsMenu = !showTagsMenu;
        if (showTagsMenu)
        {
            showCreateMenu = false;
        }
    }

    private void CloseTagsMenu() => showTagsMenu = false;

    private async Task ToggleAttributeAsync(ActivityAttribute attribute) =>
        await OnAttributeToggle.InvokeAsync(attribute);

    private async Task ClearAttributesAsync()
    {
        await OnClearAttributes.InvokeAsync();
    }

    private void ToggleCreateMenu()
    {
        showCreateMenu = !showCreateMenu;
        if (showCreateMenu)
        {
            showTagsMenu = false;
        }
    }

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
