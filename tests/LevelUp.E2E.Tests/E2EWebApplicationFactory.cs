using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LevelUp.E2E.Tests;

/// <summary>
/// Hosts the real LevelUp application on a real Kestrel TCP endpoint (not TestServer's in-memory
/// transport) so a genuine Chromium instance can navigate to it. Self-contained: does not inherit
/// from or reference any type in LevelUp.Web.Tests — only the production LevelUp.Web project.
/// Storage is an isolated temp JSON directory per instance, deleted on Dispose, exactly like the
/// isolation strategy Sprint 12.6 established for HTTP integration tests, just re-implemented here
/// in miniature since sharing the actual factory class would require a test-to-test reference.
/// </summary>
public sealed class E2EWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string storageDirectory =
        Path.Combine(Path.GetTempPath(), "levelup-e2e-tests", Guid.NewGuid().ToString("N"));

    public E2EWebApplicationFactory() => UseKestrel(port: 0);

    /// <summary>
    /// The real, OS-assigned address Kestrel bound to, populated by the base class once the server
    /// has actually started (triggered by touching <see cref="WebApplicationFactory{TEntryPoint}.Server"/>).
    /// <c>UseKestrel(int)</c> — the first-party API .NET 10's Microsoft.AspNetCore.Mvc.Testing
    /// added for exactly this scenario — makes this synchronous and deterministic: by the time the
    /// constructor call above returns control to a caller that then reads this property (after
    /// touching Server), Kestrel is already bound. No polling, no fixed delay, and since this runs
    /// in-process there is no child OS process to ever leak.
    /// </summary>
    public string ServerAddress => ClientOptions.BaseAddress.ToString().TrimEnd('/');

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["LevelUp:Storage:Directory"] = storageDirectory,
            ["LevelUp:Email:Development:Enabled"] = "false",
            // Generous limits: E2E exercises real user journeys, not the rate limiter itself
            // (already covered by Sprint 12.6's integration tests) — this just keeps it out of the way.
            ["LevelUp:RateLimiting:Login:IpPermitLimit"] = "1000",
            ["LevelUp:RateLimiting:Login:EmailPermitLimit"] = "1000",
            ["LevelUp:RateLimiting:Login:Window"] = "00:00:01",
            ["LevelUp:RateLimiting:Login:SegmentsPerWindow"] = "1"
        }));
    }

    /// <summary>
    /// Creates a confirmed, active User directly through the repository (bypassing HTTP/UI), with
    /// onboarding optionally already completed. Every E2E test that needs a signed-in starting point
    /// uses this for arrange-only setup; the actual behavior under test is always driven through the
    /// real browser afterward.
    /// </summary>
    public async Task<User> SeedUserAsync(string email, string password, bool onboardingCompleted)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILevelUpRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var nickname = $"e2e{Guid.NewGuid():N}"[..12];

        User? user = null;
        await repository.UpdateAsync(data =>
        {
            user = User.Create("E2E Test User", email, passwordService.Hash(password));
            user.ConfirmEmail(user.CreatedAtUtc);
            data.AddUser(user);
            data.CompleteUserProfile(user.Id, nickname);
            if (onboardingCompleted)
            {
                user.CompleteOnboarding();
            }
        });

        return user!;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(storageDirectory))
        {
            try
            {
                Directory.Delete(storageDirectory, recursive: true);
            }
            catch (IOException)
            {
                // Best-effort cleanup only.
            }
        }
    }
}
