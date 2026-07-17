namespace LevelUp.UI.Infrastructure;

public sealed class UserCancelledException : OperationCanceledException
{
    public UserCancelledException()
        : base("Operação cancelada pelo usuário.")
    {
    }
}
