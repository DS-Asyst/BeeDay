using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeeDay.Infrastructure.Identity;

public sealed class ResendEmailSender(
    HttpClient httpClient,
    IOptions<ResendOptions> options,
    ILogger<ResendEmailSender> logger) : IEmailSender
{
    private readonly ResendOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.Enabled)
        {
            logger.LogInformation(EmailEventIds.ProviderDisabled, "Email delivery is disabled. Identity email was suppressed.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.UserAgent.ParseAdd("BeeDay/1.0");
        request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString("N"));
        request.Content = JsonContent.Create(new
        {
            from = $"{_options.FromName} <{_options.FromAddress}>",
            to = new[] { message.Recipient },
            subject = message.Subject,
            html = message.HtmlBody,
            text = message.PlainTextBody
        });

        // "Subject" is the only per-message identifier safe to log — it distinguishes confirmation
        // from password-reset without ever touching the recipient address or token-bearing body.
        logger.LogInformation(EmailEventIds.ProviderAttempted, "Resend request attempted. Subject={Subject}", message.Subject);

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient reports its own configured Timeout as a TaskCanceledException carrying an
            // internal token, not the caller's — distinguishing "we gave up waiting" (a transient,
            // classifiable failure) from genuine caller-requested cancellation (the `when` clause
            // above lets that case propagate silently, unlogged, since it is expected/intentional).
            logger.LogError(EmailEventIds.ProviderTimedOut, "Resend request timed out. Subject={Subject}", message.Subject);
            throw;
        }
        catch (HttpRequestException exception)
        {
            // Thrown by SendAsync itself (DNS/connection failure, TLS failure, ...) — no HTTP response
            // was ever received, so this is a transient network error, not a provider rejection.
            logger.LogError(EmailEventIds.ProviderNetworkFailure, exception, "Resend request failed before a response was received. Subject={Subject}", message.Subject);
            throw;
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                var providerMessageId = await TryReadProviderMessageIdAsync(response, cancellationToken);
                logger.LogInformation(
                    EmailEventIds.ProviderAccepted,
                    "Resend accepted the request. ProviderMessageId={ProviderMessageId}",
                    providerMessageId ?? "(unavailable)");
                return;
            }

            logger.LogError(
                EmailEventIds.ProviderRejected,
                "Resend rejected the request. StatusCode={StatusCode}",
                (int)response.StatusCode);
            throw new HttpRequestException($"Resend email delivery failed with HTTP {(int)response.StatusCode}.", null, response.StatusCode);
        }
    }

    // Resend's success response is {"id": "..."} — a safe, non-secret provider message identifier,
    // useful for correlating a captured "accepted" log line with the provider's own delivery records.
    // Never throws: an unparseable/unexpected response body must not turn a successful send into a
    // reported failure.
    private static async Task<string?> TryReadProviderMessageIdAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return document.RootElement.TryGetProperty("id", out var idElement)
                ? idElement.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
