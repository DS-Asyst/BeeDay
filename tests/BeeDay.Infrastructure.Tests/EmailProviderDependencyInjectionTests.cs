using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.DependencyInjection;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

/// <summary>
/// Verifies that <c>AddBeeDayInfrastructure</c> resolves IEmailSender to exactly the provider each
/// committed appsettings*.json intends (see docs/infrastructure/06-transactional-email.md §5.1),
/// and that an ambiguous/invalid combination of the two provider flags fails at registration time
/// rather than being silently resolved.
/// </summary>
public sealed class EmailProviderDependencyInjectionTests
{
    private const string TestConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=BeeDayEmailProviderDiTests;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    public void AddBeeDayInfrastructure_DevelopmentEnvironmentSettings_ResolveDevelopmentEmailSenderExactlyOnce()
    {
        AssertResolvesExactlyOneSender(
            resendEnabled: false,
            developmentEnabled: true,
            expectedSenderType: typeof(DevelopmentEmailSender));
    }

    [Fact]
    public void AddBeeDayInfrastructure_HomologationEnvironmentSettings_ResolveDevelopmentEmailSenderExactlyOnce()
    {
        // appsettings.Homologation.json: Resend:Enabled=false, Development:Enabled=true — same
        // effective provider as Development today (the configured Directory value is what breaks
        // at send time on HMG, not provider selection; see 06-transactional-email.md §6).
        AssertResolvesExactlyOneSender(
            resendEnabled: false,
            developmentEnabled: true,
            expectedSenderType: typeof(DevelopmentEmailSender));
    }

    [Fact]
    public void AddBeeDayInfrastructure_ProductionEnvironmentSettings_ResolveResendEmailSenderExactlyOnce()
    {
        AssertResolvesExactlyOneSender(
            resendEnabled: true,
            developmentEnabled: false,
            expectedSenderType: typeof(ResendEmailSender),
            additionalSettings: new Dictionary<string, string?>
            {
                [$"{ResendOptions.SectionName}:ApiKey"] = "re_test",
                [$"{ResendOptions.SectionName}:FromAddress"] = "noreply@beeday.example"
            });
    }

    [Fact]
    public void AddBeeDayInfrastructure_WhenBothProvidersEnabled_ThrowsAtRegistrationTime()
    {
        var configuration = BuildConfiguration(resendEnabled: true, developmentEnabled: true);
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddBeeDayInfrastructure(configuration));
    }

    [Fact]
    public void AddBeeDayInfrastructure_WhenNoProviderEnabled_ThrowsAtRegistrationTime()
    {
        var configuration = BuildConfiguration(resendEnabled: false, developmentEnabled: false);
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddBeeDayInfrastructure(configuration));
    }

    private static void AssertResolvesExactlyOneSender(
        bool resendEnabled,
        bool developmentEnabled,
        Type expectedSenderType,
        Dictionary<string, string?>? additionalSettings = null)
    {
        var configuration = BuildConfiguration(resendEnabled, developmentEnabled, additionalSettings);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());

        services.AddBeeDayInfrastructure(configuration);

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IEmailSender));

        using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IEmailSender>();

        Assert.IsType(expectedSenderType, sender);
    }

    private static IConfiguration BuildConfiguration(
        bool resendEnabled,
        bool developmentEnabled,
        Dictionary<string, string?>? additionalSettings = null)
    {
        var settings = new Dictionary<string, string?>
        {
            [$"{SqlServerOptions.SectionName}:ConnectionString"] = TestConnectionString,
            [$"{ResendOptions.SectionName}:Enabled"] = resendEnabled.ToString(),
            [$"{DevelopmentEmailOptions.SectionName}:Enabled"] = developmentEnabled.ToString(),
            [$"{DevelopmentEmailOptions.SectionName}:Directory"] = "Data/Emails"
        };

        if (additionalSettings is not null)
        {
            foreach (var (key, value) in additionalSettings)
            {
                settings[key] = value;
            }
        }

        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "BeeDay.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
