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

    // EPIC 28, Sprint 28.8 (HMG Recipient Guard Negative Smoke Harness & Safety Evidence): the
    // strongest automated proof this repository can produce that "non-allowlisted recipient -> guard
    // blocks -> provider not invoked" holds at the REAL boundary production uses — not a
    // hand-rolled fake standing in for the guard (HmgRecipientGuardedEmailSender itself is the real,
    // unmodified class, resolved from the real DI graph exactly as AddBeeDayInfrastructure wires it),
    // only the deepest possible seam (ResendEmailSender's own HttpClient transport) is replaced, so a
    // regression that let a blocked recipient reach Resend would make this test fail by construction,
    // not by coincidence. Never sends over a real network, never widens AllowedRecipients, never
    // touches a real/synthetic-but-plausible external address (the recipient below is a
    // syntactically-valid but deliberately fake ".invalid" address — RFC 2606 reserved — never
    // dispatched anywhere since the guard blocks it before any transport is reached).
    [Fact]
    public async Task Host_WhenRecipientIsNotAllowlisted_TheRealResendHttpClientIsNeverInvoked()
    {
        var handler = new RecordingHttpMessageHandler();
        using var host = BuildHost(
            new Dictionary<string, string?>
            {
                [$"{ResendOptions.SectionName}:Enabled"] = "true",
                [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
                [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
                [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
                [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "true",
                [$"{HmgRecipientGuardOptions.SectionName}:AllowedRecipients:0"] = "owner@beeday.example"
            },
            services => services.AddHttpClient<ResendEmailSender>().ConfigurePrimaryHttpMessageHandler(() => handler));

        await host.StartAsync(TestContext.Current.CancellationToken);

        var sender = host.Services.GetRequiredService<IEmailSender>();
        await sender.SendAsync(
            new EmailMessage("not-allowlisted@example.invalid", "Subject", "<p>Body</p>"),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, handler.CallCount);

        await host.StopAsync(TestContext.Current.CancellationToken);
    }

    // Positive-path counterpart at the same real boundary, so a future change that broke the
    // allowlisted path could not hide behind the negative test above going green for the wrong
    // reason (e.g. the HTTP client being unreachable for every recipient, not just blocked ones).
    [Fact]
    public async Task Host_WhenRecipientIsAllowlisted_TheRealResendHttpClientIsInvokedExactlyOnce()
    {
        var handler = new RecordingHttpMessageHandler();
        using var host = BuildHost(
            new Dictionary<string, string?>
            {
                [$"{ResendOptions.SectionName}:Enabled"] = "true",
                [$"{ResendOptions.SectionName}:ApiKey"] = "re_test_only_used_in_memory_never_a_real_key",
                [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example",
                [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = "false",
                [$"{HmgRecipientGuardOptions.SectionName}:Enabled"] = "true",
                [$"{HmgRecipientGuardOptions.SectionName}:AllowedRecipients:0"] = "owner@beeday.example"
            },
            services => services.AddHttpClient<ResendEmailSender>().ConfigurePrimaryHttpMessageHandler(() => handler));

        await host.StartAsync(TestContext.Current.CancellationToken);

        var sender = host.Services.GetRequiredService<IEmailSender>();
        await sender.SendAsync(
            new EmailMessage("owner@beeday.example", "Subject", "<p>Body</p>"),
            TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.CallCount);

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

    private static IHost BuildHost(Dictionary<string, string?> settings, Action<IServiceCollection>? configureServices = null)
    {
        settings[$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString;

        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.AddInMemoryCollection(settings);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddBeeDayInfrastructure(context.Configuration);
                // Applied after AddBeeDayInfrastructure so it overrides that method's own
                // AddHttpClient<ResendEmailSender> primary handler — IHttpClientFactory composes
                // configuration for a given typed client, and the last-registered primary handler
                // factory wins, which is exactly the seam this file's negative/positive smoke tests
                // need without touching AddBeeDayInfrastructure itself.
                configureServices?.Invoke(services);
            })
            .Build();
    }

    // Counts real invocations of the transport ResendEmailSender's HttpClient would use — never
    // performs a real network call itself (always returns a canned in-memory response), so this test
    // file makes zero real HTTP requests regardless of which path (allowed/blocked) is exercised.
    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"smoke-test-message-id\"}", System.Text.Encoding.UTF8, "application/json")
            });
        }
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
