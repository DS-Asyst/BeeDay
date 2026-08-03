namespace LevelUp.Application.Common.Identity;

public interface IIdentityEmailComposer
{
    public EmailMessage ComposeEmailConfirmation(string recipient, string displayName, string rawToken);
    public EmailMessage ComposePasswordReset(string recipient, string displayName, string rawToken);
}
