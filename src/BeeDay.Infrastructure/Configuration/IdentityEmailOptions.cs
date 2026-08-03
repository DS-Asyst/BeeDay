namespace BeeDay.Infrastructure.Configuration;

public sealed class IdentityEmailOptions
{
    public const string SectionName = "BeeDay:IdentityEmail";

    public string PublicBaseUrl { get; set; } = "https://localhost:5001";
    public string ConfirmationPath { get; set; } = "/account/confirm-email";
    public string PasswordResetPath { get; set; } = "/account/reset-password";
}
