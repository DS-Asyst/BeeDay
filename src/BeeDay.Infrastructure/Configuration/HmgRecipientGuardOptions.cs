namespace BeeDay.Infrastructure.Configuration;

/// <summary>
/// Governs <see cref="Identity.HmgRecipientGuardedEmailSender"/>, which wraps <c>ResendEmailSender</c>
/// whenever Resend is the selected provider (see <see cref="EmailProviderSelector"/>). Defaults to
/// <see cref="Enabled"/> <c>true</c> so an environment that starts sending through Resend without
/// ever configuring this section fails closed — startup validation rejects an enabled guard with no
/// <see cref="AllowedRecipients"/> rather than silently allowing every recipient through. An
/// environment intended to send freely (Production) must set <c>Enabled: false</c> explicitly.
/// </summary>
public sealed class HmgRecipientGuardOptions
{
    public const string SectionName = "BeeDay:Email:HmgRecipientGuard";

    public bool Enabled { get; set; } = true;
    public IReadOnlyList<string> AllowedRecipients { get; set; } = [];
    public string SubjectPrefix { get; set; } = "[HMG] ";
}
