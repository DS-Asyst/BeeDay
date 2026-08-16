namespace BeeDay.Infrastructure.Configuration;

/// <summary>
/// Resolves <c>BeeDay:Email:Resend:Enabled</c> and <c>BeeDay:Email:Development:Enabled</c> into
/// exactly one effective <see cref="EmailProvider"/>. Both flags remain independently configurable
/// (unchanged, already-deployed keys — see docs/infrastructure/06-transactional-email.md §4.1), but
/// enabling both or neither is ambiguous configuration intent, not a safe default to fall back on
/// silently, so both cases fail fast instead of picking a winner.
/// </summary>
public static class EmailProviderSelector
{
    public static EmailProvider Resolve(bool resendEnabled, bool developmentEnabled) =>
        (resendEnabled, developmentEnabled) switch
        {
            (true, false) => EmailProvider.Resend,
            (false, true) => EmailProvider.Development,
            (true, true) => throw new InvalidOperationException(
                $"Ambiguous email provider configuration: both '{ResendOptions.SectionName}:Enabled' and '{DevelopmentEmailOptions.SectionName}:Enabled' are true. Exactly one must be enabled."),
            (false, false) => throw new InvalidOperationException(
                $"Invalid email provider configuration: both '{ResendOptions.SectionName}:Enabled' and '{DevelopmentEmailOptions.SectionName}:Enabled' are false. Exactly one must be enabled.")
        };
}
