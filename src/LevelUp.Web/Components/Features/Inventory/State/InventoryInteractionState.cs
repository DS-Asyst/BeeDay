namespace LevelUp.Web.Components.Features.Inventory.State;

public sealed class InventoryInteractionState
{
    public bool IsBusy { get; private set; }
    public string? Operation { get; private set; }

    public bool TryBegin(string operation)
    {
        if (IsBusy)
        {
            return false;
        }

        IsBusy = true;
        Operation = operation;
        return true;
    }

    public void End()
    {
        IsBusy = false;
        Operation = null;
    }
}
