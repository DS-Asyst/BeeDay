namespace LevelUp.Web.Services;

public sealed class ToastService
{
    public event Action<ToastMessage>? MessagePublished;

    public void Success(string message)
    {
        Publish(message, ToastType.Success);
    }

    public void Error(string message)
    {
        Publish(message, ToastType.Error);
    }

    public void Information(string message)
    {
        Publish(message, ToastType.Information);
    }

    private void Publish(string message, ToastType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        MessagePublished?.Invoke(new ToastMessage(message, type));
    }
}

public sealed record ToastMessage(
    string Message,
    ToastType Type);

public enum ToastType
{
    Information,
    Success,
    Error
}
