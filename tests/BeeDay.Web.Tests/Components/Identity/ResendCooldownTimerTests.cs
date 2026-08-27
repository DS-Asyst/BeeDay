using BeeDay.Web.Components.Features.Identity;

namespace BeeDay.Web.Tests.Components.Identity;

// EXP32-F012 (Sprint 32.19): ResendConfirmation.razor and EmailConfirmationSent.razor previously
// duplicated this exact countdown implementation verbatim. Uses a short real duration (not the
// production 60s) so the test stays fast and deterministic without mocking time.
public sealed class ResendCooldownTimerTests
{
    [Fact]
    public void Start_SetsSecondsRemainingImmediately()
    {
        using var cooldown = new ResendCooldownTimer(() => Task.CompletedTask);

        cooldown.Start(5);

        Assert.Equal(5, cooldown.SecondsRemaining);
    }

    [Fact]
    public async Task Start_CountsDownToZeroAndInvokesTheTickCallbackOncePerSecond()
    {
        var tickCount = 0;
        using var cooldown = new ResendCooldownTimer(() =>
        {
            tickCount++;
            return Task.CompletedTask;
        });

        cooldown.Start(2);
        await WaitUntilAsync(() => cooldown.SecondsRemaining == 0, TimeSpan.FromSeconds(5));

        Assert.Equal(0, cooldown.SecondsRemaining);
        Assert.Equal(2, tickCount);
    }

    [Fact]
    public async Task Start_CalledAgainBeforeCompletion_RestartsFromTheNewValue()
    {
        using var cooldown = new ResendCooldownTimer(() => Task.CompletedTask);

        cooldown.Start(10);
        await Task.Delay(TimeSpan.FromMilliseconds(1100), Xunit.TestContext.Current.CancellationToken);
        cooldown.Start(3);

        Assert.Equal(3, cooldown.SecondsRemaining);
        await WaitUntilAsync(() => cooldown.SecondsRemaining == 0, TimeSpan.FromSeconds(5));
        Assert.Equal(0, cooldown.SecondsRemaining);
    }

    [Fact]
    public void Dispose_BeforeAnyStart_DoesNotThrow()
    {
        var cooldown = new ResendCooldownTimer(() => Task.CompletedTask);

        var exception = Record.Exception(cooldown.Dispose);

        Assert.Null(exception);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        while (!condition())
        {
            if (cts.IsCancellationRequested)
            {
                throw new TimeoutException("Condition was not met within the timeout.");
            }

            await Task.Delay(50, Xunit.TestContext.Current.CancellationToken);
        }
    }
}
