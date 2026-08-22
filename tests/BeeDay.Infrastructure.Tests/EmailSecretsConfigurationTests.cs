using System.Text.Json;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// Proves the official Epic 26 secrets/configuration contract end to end (see
/// docs/deployment/02-runtime-configuration.md §7): Resend selected without its secret fails the
/// real host at startup, the file provider never needs one, and no committed appsettings*.json
/// ever carries a real value for the secret fields.
/// </summary>
public sealed class EmailSecretsConfigurationTests
{
    private const string TestConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=BeeDayEmailSecretsTests;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public async Task Host_WhenResendSelectedWithoutApiKey_FailsToStartPredictably()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString,
            [$"{ResendOptions.SectionName}:Enabled"] = "true",
            [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
            // Isolates this test to the secret-configuration failure this test is about — the HMG
            // recipient guard (Sprint 26.4) is a separate, independently validated concern; see
            // EmailSecretsConfigurationTests vs. HmgRecipientGuardDependencyInjectionTests.
            [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "false"
        });

        var exception = await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync(TestContext.Current.CancellationToken));

        Assert.Contains("Resend", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Host_WhenResendSelectedWithoutFromAddress_FailsToStartPredictably()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString,
            [$"{ResendOptions.SectionName}:Enabled"] = "true",
            [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
            [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "false"
        });

        await Assert.ThrowsAsync<OptionsValidationException>(
            () => host.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Host_WhenDevelopmentProviderSelected_StartsWithoutRequiringAnyResendSecret()
    {
        using var host = BuildHost(new Dictionary<string, string?>
        {
            [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString,
            [$"{ResendOptions.SectionName}:Enabled"] = "false",
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "true",
            [$"{DevelopmentEmailOptions.SectionName}:Directory"] = "Data/Emails"
        });

        await host.StartAsync(TestContext.Current.CancellationToken);
        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("appsettings.json")]
    [InlineData("appsettings.Development.json")]
    [InlineData("appsettings.Homologation.json")]
    [InlineData("appsettings.Production.json")]
    public void CommittedAppsettings_NeverCarriesAResendApiKeyValue(string fileName)
    {
        var path = Path.Combine(GetWebProjectDirectory(), fileName);
        using var document = JsonDocument.Parse(File.ReadAllText(path));

        if (!TryGetProperty(document.RootElement, ["BeeDay", "Email", "Resend", "ApiKey"], out var apiKey))
        {
            return;
        }

        Assert.Equal(string.Empty, apiKey.GetString());
    }

    private static bool TryGetProperty(JsonElement root, string[] path, out JsonElement value)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                value = default;
                return false;
            }
        }

        value = current;
        return true;
    }

    private static IHost BuildHost(Dictionary<string, string?> settings) =>
        Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(settings);
            })
            .ConfigureServices((context, services) => services.AddBeeDayInfrastructure(context.Configuration))
            .Build();

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
