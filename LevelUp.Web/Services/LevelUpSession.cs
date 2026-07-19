using LevelUp.Web.State;

namespace LevelUp.Web.Services;

/// <summary>
/// Representa a sessão da aplicação LevelUp no Blazor.
/// A integração com persistência e domínio será adicionada gradualmente.
/// </summary>
public sealed class LevelUpSession
{
    private readonly LevelUpStore _store;

    public LevelUpSession(LevelUpStore store)
    {
        _store = store;
    }

    public bool IsInitialized => _store.IsInitialized;

    public Task InitializeAsync()
    {
        if (_store.IsInitialized)
        {
            return Task.CompletedTask;
        }

        _store.MarkAsInitialized();

        return Task.CompletedTask;
    }
}
