using Microsoft.Extensions.Logging;

namespace BeeDay.Infrastructure.Diagnostics;

// EPIC 28, Sprint 28.7 (Observability Operationalization): typed EventIds for every log call site
// already present in the transactional-email path (HmgRecipientGuardedEmailSender,
// ResendEmailSender, DevelopmentEmailSender) — lets an operator filter/grep by a stable numeric id
// instead of message text, without changing what is logged or introducing a parallel logging stack.
// Infrastructure-owned because these senders live here and cannot reference BeeDay.Web.Diagnostics.
// Numbered in the 71xx block, distinct from BeeDay.Web.Diagnostics.WebEventIds' 61xx (the only other
// typed EventId in the repository, per docs/deployment/03-observability.md).
public static class EmailEventIds
{
    public static readonly EventId GuardAllowed = new(7100, nameof(GuardAllowed));
    public static readonly EventId GuardBlocked = new(7101, nameof(GuardBlocked));
    public static readonly EventId ProviderDisabled = new(7102, nameof(ProviderDisabled));
    public static readonly EventId ProviderAttempted = new(7103, nameof(ProviderAttempted));
    public static readonly EventId ProviderAccepted = new(7104, nameof(ProviderAccepted));
    public static readonly EventId ProviderRejected = new(7105, nameof(ProviderRejected));
    public static readonly EventId ProviderTimedOut = new(7106, nameof(ProviderTimedOut));
    public static readonly EventId ProviderNetworkFailure = new(7107, nameof(ProviderNetworkFailure));
    public static readonly EventId DevelopmentCaptureDisabled = new(7108, nameof(DevelopmentCaptureDisabled));
    public static readonly EventId DevelopmentCaptured = new(7109, nameof(DevelopmentCaptured));
}
