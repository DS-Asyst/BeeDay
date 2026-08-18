using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BeeDay.Web.Components.Layout;

public sealed partial class EditorialFooter : IAsyncDisposable
{
    private const string ModulePath = "./js/beeday-editorial-footer.js?v=20260818-1";

    [Inject] private IJSRuntime JS { get; set; } = default!;

    private IJSObjectReference? _module;

    private async Task ScrollToTopAsync()
    {
        try
        {
            _module ??= await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);
            await _module.InvokeVoidAsync("scrollToTop");
        }
        catch (InvalidOperationException)
        {
            // Static prerendering has no JavaScript runtime; nothing to scroll yet.
        }
        catch (JSException)
        {
            // Scrolling is progressive enhancement and must never break navigation.
        }
    }

    public async ValueTask DisposeAsync()
    {
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
