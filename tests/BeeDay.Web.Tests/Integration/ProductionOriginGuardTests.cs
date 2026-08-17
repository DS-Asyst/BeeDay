namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Proves the Program.cs startup guard that rejects a non-absolute/non-HTTPS
/// <c>BeeDay:IdentityEmail:PublicBaseUrl</c> outside Development (Epic 26, Sprint 26.5 — audits
/// <c>PublicBaseUrl</c> as the security-sensitive origin contract for every Identity email callback
/// link). Uninspected before this sprint: no test exercised this guard, the AllowedHosts guard, or
/// the DataProtectionKeysDirectory guard at all. Uses environment variables, not
/// <c>ConfigureAppConfiguration</c>, for the same reason documented on
/// <see cref="ProductionLikeWebApplicationFactory"/>: the production guard clauses in Program.cs read
/// <c>builder.Configuration</c> synchronously before <c>Build()</c>, earlier than
/// <c>WebApplicationFactory</c>'s config overlay takes effect.
/// </summary>
public sealed class ProductionOriginGuardTests
{
    [Fact]
    public void ProductionEnvironment_WithNonHttpsPublicBaseUrl_FailsToStart()
    {
        AssertStartupFails(
            publicBaseUrl: "http://h-beeday.com.br",
            expectedMessageFragment: "PublicBaseUrl must be an absolute HTTPS URL");
    }

    [Fact]
    public void ProductionEnvironment_WithRelativePublicBaseUrl_FailsToStart()
    {
        AssertStartupFails(
            publicBaseUrl: "/account/confirm-email",
            expectedMessageFragment: "PublicBaseUrl must be an absolute HTTPS URL");
    }

    [Fact]
    public void ProductionEnvironment_WithMissingPublicBaseUrl_FailsToStart()
    {
        AssertStartupFails(
            publicBaseUrl: null,
            expectedMessageFragment: "PublicBaseUrl must be an absolute HTTPS URL");
    }

    private static void AssertStartupFails(string? publicBaseUrl, string expectedMessageFragment)
    {
        // The invalid PublicBaseUrl only needs to be present in the process environment for the
        // single synchronous host-boot attempt below — restoring immediately after (rather than
        // waiting for the assertions or the end of the using block) minimizes the window in which a
        // concurrently-booting, unrelated test could read the deliberately-invalid value.
        Exception? exception;
        using (var factory = new InvalidPublicBaseUrlWebApplicationFactory(publicBaseUrl))
        {
            exception = Record.Exception(() => _ = factory.Services);
        }

        Assert.NotNull(exception);
        Assert.True(
            ContainsMessage(exception, expectedMessageFragment),
            $"Expected an exception mentioning '{expectedMessageFragment}' somewhere in the chain, got: {Flatten(exception)}");
    }

    private static bool ContainsMessage(Exception? exception, string fragment)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current.Message.Contains(fragment, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string Flatten(Exception exception) =>
        string.Join(" -> ", EnumerateChain(exception).Select(e => $"{e.GetType().Name}: {e.Message}"));

    private static IEnumerable<Exception> EnumerateChain(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            yield return current;
        }
    }

    /// <summary>
    /// Same production-guard-satisfying baseline as <see cref="ProductionLikeWebApplicationFactory"/>,
    /// except <c>PublicBaseUrl</c> is deliberately invalid (or absent) so only that one guard clause
    /// fires — <c>AllowedHosts</c> and <c>DataProtectionKeysDirectory</c> stay valid, isolating the
    /// assertion to the origin contract this sprint is auditing.
    /// </summary>
    private sealed class InvalidPublicBaseUrlWebApplicationFactory : BeeDayWebApplicationFactory
    {
        private readonly (string Key, string? Value)[] environmentVariables;
        private readonly string dataProtectionKeysDirectory =
            Path.Combine(Path.GetTempPath(), "beeday-web-tests-dp-origin-guard", Guid.NewGuid().ToString("N"));

        private readonly string?[] previousValues;
        private readonly IDisposable environmentVariableLock = ProductionEnvironmentVariableTestLock.Acquire();

        public InvalidPublicBaseUrlWebApplicationFactory(string? publicBaseUrl)
        {
            environmentVariables =
            [
                ("BeeDay__IdentityEmail__PublicBaseUrl", publicBaseUrl),
                ("AllowedHosts", "beeday.invalid;localhost"),
                ("BeeDay__Hosting__DataProtectionKeysDirectory", dataProtectionKeysDirectory),
                ("BeeDay__Email__Resend__Enabled", "false"),
                ("BeeDay__Email__Development__Enabled", "true")
            ];

            previousValues = new string?[environmentVariables.Length];
            for (var i = 0; i < environmentVariables.Length; i++)
            {
                previousValues[i] = Environment.GetEnvironmentVariable(environmentVariables[i].Key);
                Environment.SetEnvironmentVariable(environmentVariables[i].Key, environmentVariables[i].Value);
            }
        }

        protected override string EnvironmentName => "Production";

        // Already held for this instance's entire construct-through-dispose lifetime (field
        // initializer above) — CreateHost must not acquire it again on the same non-reentrant semaphore.
        protected override bool CreateHostAcquiresEnvironmentVariableLock => false;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                for (var i = 0; i < environmentVariables.Length; i++)
                {
                    Environment.SetEnvironmentVariable(environmentVariables[i].Key, previousValues[i]);
                }

                if (Directory.Exists(dataProtectionKeysDirectory))
                {
                    try
                    {
                        Directory.Delete(dataProtectionKeysDirectory, recursive: true);
                    }
                    catch (IOException)
                    {
                        // Best-effort cleanup only.
                    }
                }

                environmentVariableLock.Dispose();
            }
        }
    }
}
