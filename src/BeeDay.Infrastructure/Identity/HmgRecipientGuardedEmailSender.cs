using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Identity;

/// <summary>
/// Centralized, fail-closed boundary in front of external delivery — wraps the real
/// <c>IEmailSender</c> (currently always <c>ResendEmailSender</c>; see
/// <c>InfrastructureServiceCollectionExtensions</c>) so every current and future transactional
/// email flow is protected automatically, with no per-flow or per-handler awareness of HMG. When
/// <see cref="HmgRecipientGuardOptions.Enabled"/>, only recipients in
/// <see cref="HmgRecipientGuardOptions.AllowedRecipients"/> reach the inner sender; every other
/// recipient is silently suppressed, matching how <c>DevelopmentEmailSender</c>/<c>ResendEmailSender</c>
/// already treat "disabled" as a normal no-op rather than an error. Never logs the raw recipient
/// address in either branch.
/// </summary>
public sealed class HmgRecipientGuardedEmailSender(
    IEmailSender innerSender,
    IOptions<HmgRecipientGuardOptions> options,
    ILogger<HmgRecipientGuardedEmailSender> logger) : IEmailSender
{
    private readonly HmgRecipientGuardOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.Enabled)
        {
            await innerSender.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!IsAllowedRecipient(message.Recipient))
        {
            logger.LogWarning("HMG recipient guard blocked an outbound email: recipient is not on the allowed list.");
            return;
        }

        var guardedMessage = string.IsNullOrEmpty(_options.SubjectPrefix) || message.Subject.StartsWith(_options.SubjectPrefix, StringComparison.Ordinal)
            ? message
            : message with { Subject = _options.SubjectPrefix + message.Subject };

        logger.LogInformation("HMG recipient guard allowed an outbound email to a listed recipient.");
        await innerSender.SendAsync(guardedMessage, cancellationToken).ConfigureAwait(false);
    }

    private bool IsAllowedRecipient(string recipient) =>
        _options.AllowedRecipients.Any(allowed => string.Equals(allowed.Trim(), recipient.Trim(), StringComparison.OrdinalIgnoreCase));
}
