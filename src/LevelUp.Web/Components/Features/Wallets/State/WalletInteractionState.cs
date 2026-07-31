namespace LevelUp.Web.Components.Features.Wallets.State;

public sealed class WalletInteractionState
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
