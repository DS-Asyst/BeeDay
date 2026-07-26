using System.Net.Http.Headers;
using System.Net.Http.Json;
using LevelUp.Application.Common.Identity;
using LevelUp.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LevelUp.Infrastructure.Identity;

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
            logger.LogInformation("Email delivery is disabled. Identity email was suppressed.");
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.UserAgent.ParseAdd("LevelUp/1.0");
        request.Headers.TryAddWithoutValidation("Idempotency-Key", Guid.NewGuid().ToString("N"));
        request.Content = JsonContent.Create(new
        {
            from = $"{_options.FromName} <{_options.FromAddress}>",
            to = new[] { message.Recipient },
            subject = message.Subject,
            html = message.HtmlBody
        });

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        logger.LogError(
            "Resend rejected an identity email. StatusCode={StatusCode}",
            (int)response.StatusCode);
        throw new HttpRequestException($"Resend email delivery failed with HTTP {(int)response.StatusCode}.", null, response.StatusCode);
    }
}
