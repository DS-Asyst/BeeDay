namespace BeeDay.Web.Components.Features.Identity;

/// <summary>
/// Marker type for resolving the Identity area's resource catalog via
/// <c>IStringLocalizer&lt;IdentityResources&gt;</c>. Shared across ForgotPassword, ResetPassword,
/// ConfirmEmail, ResendConfirmation and EmailConfirmationSent — five small pages in the same
/// account-recovery/email-confirmation flow that already reuse identical link text
/// ("BACK TO SIGN IN", "SIGN IN") today, so one catalog avoids retranslating the same strings
/// five times.
/// </summary>
public sealed class IdentityResources;
