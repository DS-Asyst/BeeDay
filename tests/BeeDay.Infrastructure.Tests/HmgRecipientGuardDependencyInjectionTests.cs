using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.DependencyInjection;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// Proves the HMG recipient guard (Epic 26, Sprint 26.4) is wired at the DI/startup level exactly
/// as documented in docs/infrastructure/06-transactional-email.md §10: fails closed when Resend is
/// selected and the guard is left at its default (enabled, no allowed recipients), stays completely
/// out of the Development provider's path, and Production's committed configuration explicitly (not
/// merely by default) opts out.
/// </summary>
public sealed class HmgRecipientGuardDependencyInjectionTests
{
    private const string TestConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=BeeDayHmgGuardDiTests;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public async Task Host_WhenResendSelectedAndGuardLeftAtDefault_FailsToStartPredictably()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{ResendOptions.SectionName}:Enabled"] = "true",
            [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
            [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false"
            // BeeDay:Email:HmgRecipientGuard intentionally absent — defaults to Enabled=true with no
            // AllowedRecipients, which must fail startup rather than send unprotected.
        });

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync(TestContext.Current.CancellationToken));

        Assert.Contains("allowed recipient", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Host_WhenResendSelectedAndGuardEnabledWithRecipients_Starts()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{ResendOptions.SectionName}:Enabled"] = "true",
            [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
            [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
            [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "true",
            [$"{HmgRecipientGuardOptions.SectionName}:AllowedRecipients:0"] = "owner@beeday.example"
        });

        await host.StartAsync(TestContext.Current.CancellationToken);

        var sender = host.Services.GetRequiredService<IEmailSender>();
        Assert.IsType<HmgRecipientGuardedEmailSender>(sender);

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Host_WhenResendSelectedAndGuardExplicitlyDisabled_StartsAndSenderIsStillTheGuardedType()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{ResendOptions.SectionName}:Enabled"] = "true",
            [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
            [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
            [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "false"
        });

        await host.StartAsync(TestContext.Current.CancellationToken);

        // Always wrapped, even when disabled — the guard passes every recipient through unmodified
        // in that state (proven by HmgRecipientGuardedEmailSenderTests), rather than the DI graph
        // conditionally omitting the wrapper depending on Enabled.
        Assert.IsType<HmgRecipientGuardedEmailSender>(host.Services.GetRequiredService<IEmailSender>());

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Host_WhenDevelopmentProviderSelected_StartsWithNoHmgRecipientGuardConfigurationAtAll()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{ResendOptions.SectionName}:Enabled"] = "false",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "true",
            [$"{DevelopmentEmailOptions.SectionName}:Directory"] = "Data/Emails"
            // No BeeDay:Email:HmgRecipientGuard key anywhere — must never be consulted for this provider.
        });

        await host.StartAsync(TestContext.Current.CancellationToken);

        Assert.IsType<DevelopmentEmailSender>(host.Services.GetRequiredService<IEmailSender>());

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public void CommittedProductionAppsettings_ExplicitlyDisablesHmgRecipientGuard()
    {
        var path = Path.Combine(GetWebProjectDirectory(), "appsettings.Production.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        var enabled = document.RootElement
            .GetProperty("BeeDay").GetProperty("Email").GetProperty("HmgRecipientGuard").GetProperty("Enabled");

        Assert.Equal(JsonValueKind.False, enabled.ValueKind);
    }

    private static IHost BuildHost(Dictionary<string, string?> settings)
    {
        settings[$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString;

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(settings);
            })
            .ConfigureServices((context, services) => services.AddBeeDayInfrastructure(context.Configuration))
            .Build();
    }

    private static string GetWebProjectDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BeeDay.slnx")))
        {
            directory = directory.Parent;
        }

        return directory is null
            ? throw new InvalidOperationException("Could not locate the repository root (BeeDay.slnx) from the test output directory.")
            : Path.Combine(directory.FullName, "src", "BeeDay.Web");
    }
}
