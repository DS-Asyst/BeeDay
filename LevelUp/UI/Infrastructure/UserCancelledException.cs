namespace LevelUp.UI.Infrastructure;

public sealed class UserCancelledException : OperationCanceledException
{
    public UserCancelledException()
        : base("Operation cancelled by the user.")
    {
    }
}
