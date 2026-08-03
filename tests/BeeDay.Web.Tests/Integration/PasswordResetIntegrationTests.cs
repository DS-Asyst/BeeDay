using BeeDay.Application.Features.Identity.Commands;
using BeeDay.Application.Features.Identity.Requests;
using BeeDay.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the full password-reset flow through the real MediatR handlers
/// (RequestPasswordResetCommandHandler / ResetPasswordCommandHandler) — request/existing-vs-
/// nonexistent indistinguishability, throttling, token validity/expiry/reuse/revocation, and that a
/// successful reset actually changes what password logs in through the real /auth/login endpoint.
/// These commands have no raw HTTP endpoint (ForgotPassword.razor/ResetPassword.razor call MediatR
/// directly), so they're invoked via <see cref="ISender"/> from a plain DI scope — no
/// authenticated-user context is needed since neither command reads ICurrentUserContext. Each test
/// uses its own fresh factory instance (never shared via IClassFixture) so per-email throttle state
/// and captured emails never bleed across tests regardless of execution order.
/// </summary>
public sealed class PasswordResetIntegrationTests
{
    [Fact]
    public async Task RequestPasswordReset_ForExistingConfirmedUser_CapturesEmailWithUsableToken()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("reset-existing@levelup.invalid", "Password123!");

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new RequestPasswordResetCommand(new RequestPasswordResetRequest("reset-existing@levelup.invalid")), cancellationToken);
        }

        Assert.Equal(1, factory.CountCapturedEmails());
        var token = await factory.TryGetLatestCapturedTokenAsync(cancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(token));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ResetPasswordCommand(new ResetPasswordRequest(token!, "NewPassword123!", "NewPassword123!")), cancellationToken);
        }

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var oldPasswordAttempt = await PostLoginAsync(client, "reset-existing@levelup.invalid", "Password123!", cancellationToken);
        Assert.Contains("error=invalid", oldPasswordAttempt.Headers.Location!.ToString(), StringComparison.Ordinal);

        var newPasswordAttempt = await PostLoginAsync(client, "reset-existing@levelup.invalid", "NewPassword123!", cancellationToken);
        Assert.False(newPasswordAttempt.Headers.Location?.ToString().Contains("error=invalid", StringComparison.Ordinal) ?? false);
    }

    [Fact]
    public async Task RequestPasswordReset_ForNonexistentEmail_CompletesSilentlyWithoutCapturingEmail()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // No exception, regardless of whether the account exists — the handler must never let a
        // caller distinguish the two cases through its outcome (success/failure or timing of throw).
        await sender.Send(new RequestPasswordResetCommand(new RequestPasswordResetRequest("reset-never-existed@levelup.invalid")), cancellationToken);

        Assert.Equal(0, factory.CountCapturedEmails());
    }

    [Fact]
    public async Task RequestPasswordReset_SecondImmediateRequest_IsThrottledAndSendsNoSecondEmail()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("reset-throttled@levelup.invalid", "Password123!");

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(new RequestPasswordResetCommand(new RequestPasswordResetRequest("reset-throttled@levelup.invalid")), cancellationToken);
        await sender.Send(new RequestPasswordResetCommand(new RequestPasswordResetRequest("reset-throttled@levelup.invalid")), cancellationToken);

        Assert.Equal(1, factory.CountCapturedEmails());
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Throws()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new LevelUpWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ResetPasswordCommand(new ResetPasswordRequest("not-a-real-token", "NewPassword123!", "NewPassword123!")), cancellationToken));
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_Throws()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new LevelUpWebApplicationFactory();
        var user = await factory.SeedConfirmedUserAsync("reset-expired@levelup.invalid", "Password123!");
        var token = await factory.IssuePasswordResetTokenAsync(user.Id, expired: true);

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ResetPasswordCommand(new ResetPasswordRequest(token, "NewPassword123!", "NewPassword123!")), cancellationToken));
    }

    [Fact]
    public async Task ResetPassword_WithAlreadyUsedToken_Throws()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new LevelUpWebApplicationFactory();
        var user = await factory.SeedConfirmedUserAsync("reset-reused@levelup.invalid", "Password123!");
        var token = await factory.IssuePasswordResetTokenAsync(user.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new ResetPasswordCommand(new ResetPasswordRequest(token, "NewPassword123!", "NewPassword123!")), cancellationToken);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ResetPasswordCommand(new ResetPasswordRequest(token, "AnotherPassword123!", "AnotherPassword123!")), cancellationToken));
    }

    [Fact]
    public async Task ResetPassword_RevokesPriorOutstandingTokenForTheSameUser()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new LevelUpWebApplicationFactory();
        var user = await factory.SeedConfirmedUserAsync("reset-revoked@levelup.invalid", "Password123!");
        var firstToken = await factory.IssuePasswordResetTokenAsync(user.Id);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            // A fresh request for the same user revokes every previously issued, still-active
            // PasswordReset token (RequestPasswordResetCommandHandler calls RevokeActiveUserTokens).
            await sender.Send(new RequestPasswordResetCommand(new RequestPasswordResetRequest("reset-revoked@levelup.invalid")), cancellationToken);
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
                sender.Send(new ResetPasswordCommand(new ResetPasswordRequest(firstToken, "NewPassword123!", "NewPassword123!")), cancellationToken));
        }
    }

    private static async Task<HttpResponseMessage> PostLoginAsync(HttpClient client, string email, string password, CancellationToken cancellationToken)
    {
        var token = await AntiforgeryTestHelper.GetTokenAsync(client, "/login", cancellationToken);
        return await client.PostAsync(
            "/auth/login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password,
                ["__RequestVerificationToken"] = token
            }),
            cancellationToken);
    }
}
