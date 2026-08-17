namespace BeeDay.Application.Common.Identity;

public interface IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(string Recipient, string Subject, string HtmlBody, string? PlainTextBody = null);
