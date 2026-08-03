using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BeeDay.Web.Components.Behaviors.DragDrop;

public partial class BeeDaySortable : IAsyncDisposable
{
    private ElementReference container;
    private IJSObjectReference? module;
    private DotNetObjectReference<BeeDaySortable>? selfReference;

    [Parameter, EditorRequired] public IReadOnlyList<Guid> ItemIds { get; set; } = [];
    [Parameter, EditorRequired] public RenderFragment<Guid> ItemTemplate { get; set; } = default!;
    [Parameter, EditorRequired] public EventCallback<SortableReorderEvent> OnReorder { get; set; }
    [Parameter, EditorRequired] public string CollectionKey { get; set; } = string.Empty;
    [Parameter] public string AriaLabel { get; set; } = "Reorderable activity list";
    [Parameter] public string? Class { get; set; }
    [Parameter] public int VirtualizationThreshold { get; set; } = 30;
    [Parameter] public float ItemSize { get; set; } = 86f;
    [Parameter] public int OverscanCount { get; set; } = 4;
    [Parameter] public Guid? RemovingItemId { get; set; }

    private bool UseVirtualization => ItemIds.Count >= VirtualizationThreshold;
    private ICollection<Guid> VirtualizedItemIds => ItemIds as ICollection<Guid> ?? ItemIds.ToArray();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/beeday-sortable.js?v=20260721-f13-dragfix");
        selfReference = DotNetObjectReference.Create(this);
        await module.InvokeVoidAsync("initialize", container, selfReference);
    }

    [JSInvokable]
    public Task NotifyReorderAsync(string itemId, string targetItemId, bool placeAfter) =>
        OnReorder.InvokeAsync(new SortableReorderEvent(itemId, targetItemId, placeAfter));

    public async ValueTask DisposeAsync()
    {
        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("dispose", container);
                await module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already disconnected.
            }
        }

        selfReference?.Dispose();
    }
}
