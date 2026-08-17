namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Serializes every real-host startup (<see cref="BeeDayWebApplicationFactory.CreateHost"/>, every
/// subclass) against any factory that mutates process-wide environment variables affecting
/// Program.cs's pre-<c>Build()</c> startup guards (<c>PublicBaseUrl</c>, <c>AllowedHosts</c>,
/// <c>DataProtectionKeysDirectory</c>, Resend/Development toggles) — currently
/// <see cref="ProductionLikeWebApplicationFactory"/> and <c>ProductionOriginGuardTests</c>'s
/// factory. xUnit parallelizes across test classes by default; environment variables are process-wide,
/// so without this, an ordinary <see cref="BeeDayWebApplicationFactory"/>-based test booting
/// concurrently with one of those two factories can read the other test's (possibly deliberately
/// invalid) value and fail with a spurious <c>OptionsValidationException</c> far from the actual
/// cause (Epic 26, Sprint 26.5).
/// </summary>
/// <remarks>
/// Deliberately NOT reentrant. An earlier version tracked acquisition depth via
/// <see cref="AsyncLocal{T}"/> so a factory that already held the lock in its constructor could
/// acquire it again inside <see cref="BeeDayWebApplicationFactory.CreateHost"/> without deadlocking
/// itself. That relies on ExecutionContext/AsyncLocal reliably flowing from constructor to the later
/// <c>.Services</c>/<c>CreateClient()</c> call on the exact same logical chain — true for a single
/// isolated test, but under real xUnit parallel execution (many interleaved test classes on a shared
/// thread pool) it was observed to fail intermittently, causing spurious cross-test
/// <c>OptionsValidationException</c> failures that did not reproduce when the same test ran alone.
/// The double-acquisition problem is solved instead with an explicit, non-timing-dependent signal:
/// <see cref="BeeDayWebApplicationFactory.CreateHostAcquiresEnvironmentVariableLock"/>, overridden to
/// <see langword="false"/> by factories that already hold this lock for their whole lifetime.
/// </remarks>
internal static class ProductionEnvironmentVariableTestLock
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    public static IDisposable Acquire()
    {
        Gate.Wait();
        return new Releaser();
    }

    private sealed class Releaser : IDisposable
    {
        private bool released;

        public void Dispose()
        {
            if (released)
            {
                return;
            }

            released = true;
            Gate.Release();
        }
    }
}
