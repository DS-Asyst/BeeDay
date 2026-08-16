using BeeDay.Web.Resources;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Services;

public sealed class ToastService(IStringLocalizer<SharedResources> localizer)
{
    private readonly List<ToastMessage> messages = [];

    public event Action? Changed;

    public IReadOnlyList<ToastMessage> Messages => messages;

    public void ShowSuccess(string message, string? title = null) =>
        Show(message, title ?? localizer["ToastDefaultSuccessTitle"], ToastVariant.Success);

    public void ShowError(string message, string? title = null) =>
        Show(message, title ?? localizer["ToastDefaultErrorTitle"], ToastVariant.Error, TimeSpan.FromSeconds(7));

    public void ShowInfo(string message, string? title = null) =>
        Show(message, title ?? localizer["ToastDefaultInfoTitle"], ToastVariant.Info);

    public void Remove(Guid id)
    {
        if (messages.RemoveAll(item => item.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    private void Show(
        string message,
        string title,
        ToastVariant variant,
        TimeSpan? duration = null)
    {
        var toast = new ToastMessage(Guid.NewGuid(), title, message, variant);
        messages.Add(toast);
        Changed?.Invoke();
        _ = RemoveAfterDelayAsync(toast.Id, duration ?? TimeSpan.FromSeconds(4));
    }

    private async Task RemoveAfterDelayAsync(Guid id, TimeSpan duration)
    {
        await Task.Delay(duration);
        Remove(id);
    }
}

public sealed record ToastMessage(
    Guid Id,
    string Title,
    string Message,
    ToastVariant Variant);

public enum ToastVariant
{
    Success,
    Error,
    Info
}
