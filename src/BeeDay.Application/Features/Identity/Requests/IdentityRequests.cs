namespace BeeDay.Application.Features.Identity.Requests;

public sealed record ConfirmEmailRequest(string Token);
public sealed record ResendEmailConfirmationRequest(string Email);
public sealed record RequestPasswordResetRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword, string ConfirmNewPassword);
