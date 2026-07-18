namespace LevelUp.Web.State;

/// <summary>
/// Centraliza o estado compartilhado da interface Blazor.
/// Não contém regras de negócio do domínio.
/// </summary>
public sealed class LevelUpStore
{
    public event Action? StateChanged;

    public bool IsInitialized { get; private set; }

    public void MarkAsInitialized()
    {
        if (IsInitialized)
        {
            return;
        }

        IsInitialized = true;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }
}
