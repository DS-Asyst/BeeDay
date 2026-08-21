using System.Text.Json;
using System.Text.RegularExpressions;
using BeeDay.Application.Common.Contracts;
using BeeDay.Application.Common.Security;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;
using BeeDay.Domain.Experience;
using BeeDay.Infrastructure.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BeeDay.E2E.Tests;

/// <summary>
/// Hosts the real BeeDay application on a real Kestrel TCP endpoint (not TestServer's in-memory
/// transport) so a genuine Chromium instance can navigate to it. Self-contained: does not inherit
/// from or reference any type in BeeDay.Web.Tests — only the production BeeDay.Web project.
/// Storage is an isolated, disposable SQL Server LocalDB database per instance (migrated on startup,
/// dropped on Dispose) — the Sprint 14.6 SQL Server cutover replaced the JSON temp-directory isolation
/// this class used before, re-implemented here in miniature since sharing the actual Web.Tests factory
/// class would require a test-to-test reference.
/// </summary>
public sealed class E2EWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly Regex TokenPattern = new("[?&]token=([^&\"]+)", RegexOptions.Compiled);

    private readonly string connectionString =
        $"Server=(localdb)\\mssqllocaldb;Database=BeeDay_E2ETests_{Guid.NewGuid():N};Trusted_Connection=True;TrustServerCertificate=True;";

    // Enabled unconditionally (not just for the one test class that reads it) — every other E2E
    // test seeds an already-confirmed user directly through the repository and never triggers an
    // identity email, so this only ever produces files for AccountLifecycleTests' real
    // registration/confirmation journey. Mirrors Web.Tests' EmailCaptureWebApplicationFactory,
    // reimplemented here since this factory deliberately never references BeeDay.Web.Tests.
    private readonly string emailDirectoryRelativePath =
        Path.Combine("App_Data", "e2e-email-capture", Guid.NewGuid().ToString("N"));

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

    // Cached on first access, same reasoning as EmailCaptureWebApplicationFactory: Services is torn
    // down by the time Dispose runs, so ContentRootPath can no longer be resolved from DI then.
    private string? cachedEmailCaptureDirectory;

    private string EmailCaptureDirectory => cachedEmailCaptureDirectory ??=
        Path.Combine(Services.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>().ContentRootPath, emailDirectoryRelativePath);

    /// <summary>
    /// Returns the raw token embedded in the action link of the most recently captured identity
    /// email addressed to <paramref name="recipientEmail"/>, or null if none was captured yet.
    /// Filters by the companion ".json" metadata file's recorded Recipient — this factory (and its
    /// capture directory) is shared across every test method in the owning test class via
    /// IClassFixture, so "the single latest file in the directory" would race against any other
    /// concurrently- or previously-run test in the same class that also triggered an identity email.
    /// </summary>
    public async Task<string?> TryGetCapturedTokenForRecipientAsync(string recipientEmail, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(EmailCaptureDirectory))
        {
            return null;
        }

        var matches = new List<(string HtmlFile, DateTimeOffset CapturedAtUtc)>();
        foreach (var metadataFile in new DirectoryInfo(EmailCaptureDirectory).GetFiles("*.json"))
        {
            var json = await File.ReadAllTextAsync(metadataFile.FullName, cancellationToken);
            var metadata = JsonSerializer.Deserialize<CapturedEmailMetadata>(json);
            if (metadata is null || !string.Equals(metadata.Recipient, recipientEmail, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            matches.Add((Path.Combine(EmailCaptureDirectory, metadata.HtmlFile), metadata.CapturedAtUtc));
        }

        var latest = matches.OrderByDescending(match => match.CapturedAtUtc).FirstOrDefault();
        if (latest.HtmlFile is null)
        {
            return null;
        }

        var html = await File.ReadAllTextAsync(latest.HtmlFile, cancellationToken);
        var tokenMatch = TokenPattern.Match(html);
        return tokenMatch.Success ? Uri.UnescapeDataString(tokenMatch.Groups[1].Value) : null;
    }

    private sealed record CapturedEmailMetadata(string Recipient, string HtmlFile, DateTimeOffset CapturedAtUtc);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BeeDay:Persistence:SqlServer:ConnectionString"] = connectionString,
            ["BeeDay:Email:Development:Enabled"] = "true",
            ["BeeDay:Email:Development:Directory"] = emailDirectoryRelativePath,
            // Generous limits: E2E exercises real user journeys, not the rate limiter itself
            // (already covered by Sprint 12.6's integration tests) — this just keeps it out of the way.
            ["BeeDay:RateLimiting:Login:IpPermitLimit"] = "1000",
            ["BeeDay:RateLimiting:Login:EmailPermitLimit"] = "1000",
            ["BeeDay:RateLimiting:Login:Window"] = "00:00:01",
            ["BeeDay:RateLimiting:Login:SegmentsPerWindow"] = "1"
        }));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
        using var context = contextFactory.CreateDbContext();
        context.Database.Migrate();

        return host;
    }

    /// <summary>
    /// Creates a confirmed, active User directly through the repository (bypassing HTTP/UI), with
    /// onboarding optionally already completed. Every E2E test that needs a signed-in starting point
    /// uses this for arrange-only setup; the actual behavior under test is always driven through the
    /// real browser afterward. <paramref name="initialExperience"/> seeds a starting XP total (via
    /// the unconditional, non-deduplicated <c>User.AddExperience</c>, never the dedup-checked
    /// <c>TryAddExperience</c>) so a test can position a user close to a level-up boundary before
    /// driving the actual level-up action through the real browser.
    /// </summary>
    public async Task<User> SeedUserAsync(string email, string password, bool onboardingCompleted, long? initialExperience = null)
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var nickname = $"e2e{Guid.NewGuid():N}"[..12];

        var user = User.Create("E2E Test User", email, passwordService.Hash(password));
        user.ConfirmEmail(user.CreatedAtUtc);
        user.CompleteProfile(nickname, avatar: null);
        if (onboardingCompleted)
        {
            user.CompleteOnboarding();
        }

        if (initialExperience is long amount)
        {
            user.AddExperience(
                ExperienceReward.Create(amount),
                ExperienceSource.Create(ExperienceSourceType.Task, Guid.NewGuid()));
        }

        await repository.AddAsync(user);

        return user;
    }

    // WebApplicationFactory<TEntryPoint> implements both IDisposable and IAsyncDisposable — xunit's
    // IClassFixture support prefers the async path when a fixture implements IAsyncDisposable, so
    // DisposeAsync (not Dispose(bool)) is what actually runs at teardown for these tests. Both are
    // overridden so the database is dropped whichever path a caller takes; each is independently safe
    // to call (EnsureDeleted[Async] is a no-op if the database is already gone).
    public override async ValueTask DisposeAsync()
    {
        var captureDirectory = cachedEmailCaptureDirectory;
        await DropDatabaseBestEffortAsync();
        await base.DisposeAsync();

        if (captureDirectory is not null)
        {
            DeleteDirectoryBestEffort(captureDirectory);
        }
    }

    protected override void Dispose(bool disposing)
    {
        var captureDirectory = cachedEmailCaptureDirectory;

        if (disposing)
        {
            DropDatabaseBestEffort();
        }

        base.Dispose(disposing);

        if (disposing && captureDirectory is not null)
        {
            DeleteDirectoryBestEffort(captureDirectory);
        }
    }

    // Same reasoning as Web.Tests' EmailCaptureWebApplicationFactory: capture files were just
    // written moments earlier in this same process, and a file handle can still be settling on
    // Windows — a handful of immediate retries clears that up reliably.
    private static void DeleteDirectoryBestEffort(string directory)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            try
            {
                Directory.Delete(directory, recursive: true);
                return;
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                Thread.Sleep(50);
            }
        }
    }

    // See BeeDayWebApplicationFactory.DropDatabaseBestEffort for why this retries: SQL Server LocalDB
    // refuses to DROP DATABASE while this same process still holds a pooled connection to it, and that
    // connection isn't always released the instant the last test method returns.
    private async Task DropDatabaseBestEffortAsync()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
                await using var context = await contextFactory.CreateDbContextAsync();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                await context.Database.EnsureDeletedAsync();
                return;
            }
            catch (Exception)
            {
                await Task.Delay(100);
            }
        }
    }

    private void DropDatabaseBestEffort()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var scope = Services.CreateScope();
                var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BeeDayDbContext>>();
                using var context = contextFactory.CreateDbContext();

                Microsoft.Data.SqlClient.SqlConnection.ClearAllPools();
                context.Database.EnsureDeleted();
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
            }
        }
    }
}
