using BeeDay.Domain.Enums;

namespace BeeDay.Application.Common.Identity;

public interface IIdentityEmailComposer
{
    /// <summary>
    /// <paramref name="language"/> is the recipient's <see cref="Domain.Entities.User.Language"/> at
    /// the time the message is composed — the only culture source approved for transactional email
    /// (ADR-006, docs/epics/28-transactional-email-experience/README.md, Sprint 28.2). Callers must
    /// always pass the actual recipient's persisted language, never a request-scoped or ambient
    /// culture: this composer runs in Infrastructure, outside any HTTP request, and must not depend on
    /// one existing.
    /// </summary>
    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken, UserLanguage language);
    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken, UserLanguage language);
}
