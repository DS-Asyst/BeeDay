using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Identity;

public sealed class DevelopmentEmailSender(
    IHostEnvironment environment,
    IOptions<DevelopmentEmailOptions> options,
    ILogger<DevelopmentEmailSender> logger) : IEmailSender
{
    private readonly DevelopmentEmailOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.Enabled)
        {
            logger.LogInformation("Development email capture is disabled. Suppressed email to {Recipient}.", EmailAddressLogMasking.Mask(message.Recipient));
            return;
        }

        var directory = ResolveDirectory(environment.ContentRootPath, _options.Directory);

        Directory.CreateDirectory(directory);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        var id = Guid.NewGuid().ToString("N")[..8];
        var baseName = $"{timestamp}-{id}";
        var htmlPath = Path.Combine(directory, $"{baseName}.html");
        var metadataPath = Path.Combine(directory, $"{baseName}.json");

        await File.WriteAllTextAsync(htmlPath, message.HtmlBody, cancellationToken);

        string? plainTextFileName = null;
        if (message.PlainTextBody is { Length: > 0 })
        {
            var plainTextPath = Path.Combine(directory, $"{baseName}.txt");
            await File.WriteAllTextAsync(plainTextPath, message.PlainTextBody, cancellationToken);
            plainTextFileName = Path.GetFileName(plainTextPath);
        }

        var metadata = JsonSerializer.Serialize(new
        {
            message.Recipient,
            message.Subject,
            HtmlFile = Path.GetFileName(htmlPath),
            PlainTextFile = plainTextFileName,
            CapturedAtUtc = DateTimeOffset.UtcNow
        }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataPath, metadata, cancellationToken);

        logger.LogInformation(
            "Development email captured for {Recipient}. Preview: {PreviewPath}",
            EmailAddressLogMasking.Mask(message.Recipient),
            htmlPath);
    }

    // Epic 26, Sprint 26.9 fix for the Sprint 26.1-proven HMG root cause
    // (docs/infrastructure/06-transactional-email.md §6): a relative Directory must still resolve
    // inside the content root — the original point of this guard, protecting against a misconfigured
    // relative value escaping via ".." segments. An absolute Directory is a deliberate operator
    // choice (e.g. HMG's externally-provisioned C:\Apps\BeeDay-Data\Emails, chosen so captured emails
    // survive a redeploy, exactly like Data Protection Keys and the Event Journal already do) and is
    // trusted as-is — the content-root containment check does not apply to it at all, rather than
    // being weakened for the relative case it still fully protects.
    private static string ResolveDirectory(string contentRootPath, string configuredDirectory)
    {
        if (Path.IsPathRooted(configuredDirectory))
        {
            return Path.GetFullPath(configuredDirectory);
        }

        var contentRoot = Path.GetFullPath(contentRootPath);
        var directory = Path.GetFullPath(Path.Combine(contentRoot, configuredDirectory));
        var contentRootPrefix = contentRoot.EndsWith(Path.DirectorySeparatorChar)
            ? contentRoot
            : contentRoot + Path.DirectorySeparatorChar;

        if (!directory.StartsWith(contentRootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("A relative development email directory must resolve inside the application content root.");
        }

        return directory;
    }
}
