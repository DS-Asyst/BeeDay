using BeeDay.Application.Features.Identity.Handlers;
using BeeDay.Domain.Entities;
using BeeDay.Domain.Enums;

namespace BeeDay.Application.Common.Identity;

public interface IEmailConfirmationIssuer
{
    /// <summary>
    /// Builds a fresh email-confirmation token for <paramref name="user"/> and the message to deliver
    /// it — pure computation, no persistence. The caller (always account creation, where the User has no
    /// prior tokens) is responsible for adding the returned token via
    /// <c>IUserTokenRepository.AddAsync</c>/<c>IUnitOfWork</c> and sending the message. Previously took
    /// and mutated <c>LevelUpData</c> directly (including revoking prior active tokens, always a no-op
    /// for a brand-new User) — a violation of Contract-First flagged in
    /// docs/architecture/07-persistence-contracts.md §5.1, corrected here on the Sprint 14.6 cutover.
    /// </summary>
    public (UserToken Token, EmailMessage Message) Issue(User user);
}

public sealed class EmailConfirmationIssuer(
    IUserTokenService tokenService,
    IIdentityEmailComposer emailComposer,
    IClock clock) : IEmailConfirmationIssuer
{
    public (UserToken Token, EmailMessage Message) Issue(User user)
    {
        var now = clock.UtcNow;
        var rawToken = tokenService.GenerateToken();
        var token = UserToken.Create(
            user.Id,
            UserTokenType.EmailConfirmation,
            tokenService.HashToken(rawToken),
            now,
            now.Add(IdentityTokenLifetimes.EmailConfirmation));
        var message = emailComposer.ComposeEmailConfirmation(user.Email, user.Name, rawToken);
        return (token, message);
    }
}
