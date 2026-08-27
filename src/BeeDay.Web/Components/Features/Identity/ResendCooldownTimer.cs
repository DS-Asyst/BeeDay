namespace BeeDay.Web.Components.Features.Identity;

/// <summary>
/// EXP32-F012 (Sprint 32.19): the 60s resend-confirmation cooldown countdown, previously duplicated
/// verbatim in ResendConfirmation.razor and EmailConfirmationSent.razor (same PeriodicTimer/
/// CancellationTokenSource lifecycle, differing only in which mutation each page sends). Shared here
/// as a plain disposable helper rather than a UI component - the two pages render their own distinct
/// markup around the shared countdown, so there is no common markup to factor out, only the timer.
/// </summary>
public sealed class ResendCooldownTimer(Func<Task> onTick) : IDisposable
{
    private PeriodicTimer? timer;
    private CancellationTokenSource? cts;

    public int SecondsRemaining { get; private set; }

    public void Start(int seconds = 60)
    {
        SecondsRemaining = seconds;
        cts?.Cancel();
        cts?.Dispose();
        timer?.Dispose();
        cts = new CancellationTokenSource();
        timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _ = RunAsync(cts.Token);
    }

    private async Task RunAsync(CancellationToken token)
    {
        try
        {
            while (SecondsRemaining > 0 && await timer!.WaitForNextTickAsync(token))
            {
                SecondsRemaining--;
                await onTick();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
        timer?.Dispose();
    }
}
