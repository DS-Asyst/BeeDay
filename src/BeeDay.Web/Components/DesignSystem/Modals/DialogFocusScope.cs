using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BeeDay.Web.Components.DesignSystem.Modals;

public sealed class DialogFocusScope : ComponentBase, IAsyncDisposable
{
    private const string ModulePath = "./js/beeday-dialog-focus.js?v=20260827-1";

    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public bool Active { get; set; }
    [Parameter, EditorRequired] public string TargetId { get; set; } = string.Empty;
    [Parameter] public string? InitialFocusSelector { get; set; }
    [Parameter] public string? FallbackFocusSelector { get; set; }

    private IJSObjectReference? _module;
    private bool _activated;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Active && !_activated)
        {
            try
            {
                _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
                _activated = await _module.InvokeAsync<bool>("activate", TargetId, InitialFocusSelector, FallbackFocusSelector);
            }
            catch (InvalidOperationException)
            {
                // Static prerendering has no JavaScript runtime. The interactive instance retries.
            }
            catch (JSException)
            {
                // Focus management is progressive enhancement and must never break dialog content.
            }
        }
        else if (!Active && _activated)
        {
            await DeactivateAsync();
        }
    }

    private async ValueTask DeactivateAsync()
    {
        if (_module is null || !_activated)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("deactivate", TargetId);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (JSException)
        {
        }
        finally
        {
            _activated = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DeactivateAsync();

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
            catch (JSException)
            {
            }
        }
    }
}
