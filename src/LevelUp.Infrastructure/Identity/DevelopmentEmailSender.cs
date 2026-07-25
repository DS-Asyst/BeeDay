using System.Text.Json;
using LevelUp.Application.Common.Identity;
using LevelUp.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Identity;

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
            logger.LogInformation("Development email capture is disabled. Suppressed email to {Recipient}.", message.Recipient);
            return;
        }

        var contentRoot = Path.GetFullPath(environment.ContentRootPath);
        var directory = Path.GetFullPath(Path.Combine(contentRoot, _options.Directory));
        var contentRootPrefix = contentRoot.EndsWith(Path.DirectorySeparatorChar)
            ? contentRoot
            : contentRoot + Path.DirectorySeparatorChar;

        if (!directory.StartsWith(contentRootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The development email directory must remain inside the application content root.");
        }

        Directory.CreateDirectory(directory);

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        var id = Guid.NewGuid().ToString("N")[..8];
        var baseName = $"{timestamp}-{id}";
        var htmlPath = Path.Combine(directory, $"{baseName}.html");
        var metadataPath = Path.Combine(directory, $"{baseName}.json");

        await File.WriteAllTextAsync(htmlPath, message.HtmlBody, cancellationToken);
        var metadata = JsonSerializer.Serialize(new
        {
            message.Recipient,
            message.Subject,
            HtmlFile = Path.GetFileName(htmlPath),
            CapturedAtUtc = DateTimeOffset.UtcNow
        }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(metadataPath, metadata, cancellationToken);

        logger.LogInformation(
            "Development email captured for {Recipient}. Preview: {PreviewPath}",
            message.Recipient,
            htmlPath);
    }
}
