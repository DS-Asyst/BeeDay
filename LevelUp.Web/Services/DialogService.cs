namespace LevelUp.Web.Services;

public sealed class DialogService
{
    public event Action<DialogRequest>? DialogRequested;

    public void OpenConfirmation(
        string title,
        string message,
        Func<Task> onConfirm)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(onConfirm);

        DialogRequested?.Invoke(
            new DialogRequest(
                title,
                message,
                onConfirm));
    }
}

public sealed record DialogRequest(
    string Title,
    string Message,
    Func<Task> OnConfirm);
