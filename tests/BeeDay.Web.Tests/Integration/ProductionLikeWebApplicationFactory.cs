namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Boots the app in the Production environment (satisfying every production-only startup guard
/// in Program.cs with harmless test values) so cookie/HSTS/HTTPS-redirect behavior that only
/// differs in Production — e.g. <c>CookieSecurePolicy.Always</c> vs <c>SameAsRequest</c> — can be
/// verified for real rather than assumed from reading the code.
/// </summary>
/// <remarks>
/// Program.cs's production guard clauses read <c>builder.Configuration</c> synchronously,
/// deliberately, before <c>Build()</c> — a correct fail-fast safety check, not a bug, so this
/// class does not ask Program.cs to change. WebApplicationFactory's own
/// <c>ConfigureAppConfiguration</c> hook only takes effect around <c>Build()</c>, too late for
/// that eager read; environment variables, read by <c>WebApplication.CreateBuilder</c> itself at
/// the very start, are not. They are set only for the lifetime of this instance and restored on
/// dispose, since they are process-wide state.
/// </remarks>
public sealed class ProductionLikeWebApplicationFactory : BeeDayWebApplicationFactory
{
    private readonly (string Key, string Value)[] requiredEnvironmentVariables;
    private readonly string dataProtectionKeysDirectory =
        Path.Combine(Path.GetTempPath(), "beeday-web-tests-dp", Guid.NewGuid().ToString("N"));

    private readonly string?[] previousValues;
    private readonly IDisposable environmentVariableLock = ProductionEnvironmentVariableTestLock.Acquire();

    public ProductionLikeWebApplicationFactory()
    {
        requiredEnvironmentVariables =
        [
            ("BeeDay__IdentityEmail__PublicBaseUrl", "https://beeday.invalid"),
            // TestServer's default client sends Host: localhost; include it explicitly alongside
            // the "real" host so ASP.NET Core's host-filtering middleware doesn't reject requests
            // — AllowedHosts still lists specific hosts, never a wildcard.
            ("AllowedHosts", "beeday.invalid;localhost"),
            ("BeeDay__Hosting__DataProtectionKeysDirectory", dataProtectionKeysDirectory),
            // appsettings.Production.json enables Resend (real email delivery, needs secrets we
            // don't have here); the Development-capture sender is enough for these tests.
            // EmailProviderSelector (Epic 26, Sprint 26.2) requires exactly one of the two flags —
            // Production.json sets Development:Enabled=false, so it must be forced true here too.
            ("BeeDay__Email__Resend__Enabled", "false"),
            ("BeeDay__Email__Development__Enabled", "true")
        ];

        previousValues = new string?[requiredEnvironmentVariables.Length];
        for (var i = 0; i < requiredEnvironmentVariables.Length; i++)
        {
            previousValues[i] = Environment.GetEnvironmentVariable(requiredEnvironmentVariables[i].Key);
            Environment.SetEnvironmentVariable(requiredEnvironmentVariables[i].Key, requiredEnvironmentVariables[i].Value);
        }
    }

    protected override string EnvironmentName => "Production";

    // Already held for this instance's entire construct-through-dispose lifetime (field initializer
    // above) — CreateHost must not acquire it again on the same non-reentrant semaphore.
    protected override bool CreateHostAcquiresEnvironmentVariableLock => false;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            for (var i = 0; i < requiredEnvironmentVariables.Length; i++)
            {
                Environment.SetEnvironmentVariable(requiredEnvironmentVariables[i].Key, previousValues[i]);
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
