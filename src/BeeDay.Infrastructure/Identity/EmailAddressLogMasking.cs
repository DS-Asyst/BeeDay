namespace BeeDay.Infrastructure.Identity;

/// <summary>
/// One small, shared masking rule for the one place an email sender's own log line still names a
/// recipient at all (<see cref="DevelopmentEmailSender"/> — Epic 26, Sprint 26.7). Not a general PII
/// framework: <see cref="ResendEmailSender"/> and <see cref="HmgRecipientGuardedEmailSender"/> already
/// never log the recipient address in any form, which remains the preferred pattern; this exists only
/// for the one log line where the local part still carries diagnostic value (matching which captured
/// file belongs to which local dev test run) without printing a full address.
/// </summary>
internal static class EmailAddressLogMasking
{
    public static string Mask(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
        {
            return "***";
        }

        var localPart = email[..atIndex];
        var domain = email[(atIndex + 1)..];
        var visible = localPart.Length < 2 ? localPart[..1] : localPart[..2];
        return $"{visible}***@{domain}";
    }
}
