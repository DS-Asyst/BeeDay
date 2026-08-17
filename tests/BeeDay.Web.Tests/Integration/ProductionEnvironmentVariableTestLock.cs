namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Serializes every <see cref="ProductionLikeWebApplicationFactory"/> instance's
/// mutate-boot-restore lifecycle against every other one. xUnit parallelizes across test classes by
/// default; the environment variables this factory mutates
/// (<c>BeeDay__IdentityEmail__PublicBaseUrl</c> etc.) are process-wide, so two overlapping instances
/// could otherwise race on the same keys. Every value this factory ever sets is valid (it exists to
/// satisfy Program.cs's production guards, not violate them), so this only needed to prevent a
/// harmless collision — nothing here needs to protect against a concurrently-booting unrelated host
/// observing a deliberately invalid value; that scenario (Epic 26, Sprint 26.5/26.6 —
/// <c>ProductionOriginGuardTests</c>, which intentionally sets invalid <c>PublicBaseUrl</c> values to
/// prove Program.cs's startup guard fires) is tested by launching the real app as an out-of-process
/// child instead, which has its own isolated environment and cannot race with this process's tests at
/// all — see <c>ProductionOriginGuardTests.cs</c>. An earlier, more ambitious version of this lock
/// tried to also protect ordinary in-process hosts against exactly that scenario (reentrant via
/// <see cref="AsyncLocal{T}"/>, then via an explicit instance-level opt-out flag); both were removed
/// after repeated intermittent failures under real parallel xUnit execution that did not reproduce in
/// isolation — the out-of-process test design sidesteps the problem entirely instead of attempting to
/// synchronize it away in-process.
/// </summary>
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
