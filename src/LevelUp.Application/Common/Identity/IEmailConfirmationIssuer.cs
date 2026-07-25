using LevelUp.Application.Features.Identity.Handlers;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Enums;

namespace LevelUp.Application.Common.Identity;

public interface IEmailConfirmationIssuer
{
    public EmailMessage Issue(LevelUpData data, User user);
}

public sealed class EmailConfirmationIssuer(
    IUserTokenService tokenService,
    IIdentityEmailComposer emailComposer,
    IClock clock) : IEmailConfirmationIssuer
{
    public EmailMessage Issue(LevelUpData data, User user)
    {
        var now = clock.UtcNow;
        data.RevokeActiveUserTokens(user.Id, UserTokenType.EmailConfirmation, now);
        var rawToken = tokenService.GenerateToken();
        data.AddUserToken(UserToken.Create(
            user.Id,
            UserTokenType.EmailConfirmation,
            tokenService.HashToken(rawToken),
            now,
            now.Add(IdentityTokenLifetimes.EmailConfirmation)));
        return emailComposer.ComposeEmailConfirmation(user.Email, user.Name, rawToken);
    }
}
