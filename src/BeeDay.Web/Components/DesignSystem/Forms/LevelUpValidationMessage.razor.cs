using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BeeDay.Web.Components.DesignSystem.Forms;

public partial class LevelUpValidationMessage<TValue> : IDisposable
{
    [CascadingParameter] private EditContext? EditContext { get; set; }
    [Parameter, EditorRequired] public Expression<Func<TValue>> For { get; set; } = default!;

    private FieldIdentifier _fieldIdentifier;
    private IReadOnlyList<string> _messages = Array.Empty<string>();
    private EditContext? _subscribedContext;

    protected override void OnParametersSet()
    {
        if (EditContext is null)
        {
            throw new InvalidOperationException($"{nameof(LevelUpValidationMessage<TValue>)} requires a cascading EditContext.");
        }
        _fieldIdentifier = FieldIdentifier.Create(For);
        if (!ReferenceEquals(_subscribedContext, EditContext))
        {
            if (_subscribedContext is not null)
            {
                _subscribedContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }
            _subscribedContext = EditContext;
            _subscribedContext.OnValidationStateChanged += HandleValidationStateChanged;
        }
        RefreshMessages();
    }

    private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs args)
    {
        RefreshMessages();
        _ = InvokeAsync(StateHasChanged);
    }

    private void RefreshMessages() => _messages = EditContext?.GetValidationMessages(_fieldIdentifier).ToArray() ?? Array.Empty<string>();

    public void Dispose()
    {
        if (_subscribedContext is not null)
        {
            _subscribedContext.OnValidationStateChanged -= HandleValidationStateChanged;
        }
    }
}
