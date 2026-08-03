using BeeDay.Application.Features.Identity.Commands;
using BeeDay.Application.Features.Identity.Requests;
using BeeDay.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BeeDay.Web.Tests.Integration;

/// <summary>
/// Exercises the full email-confirmation flow through the real MediatR handlers
/// (ConfirmEmailCommandHandler / ResendEmailConfirmationCommandHandler): token validity, expiry,
/// safe reuse rejection, revocation on resend, existence-indistinguishability of resend for an
/// already-confirmed/nonexistent address, and that login is actually gated on confirmation before
/// and after. Same infrastructure and rationale as PasswordResetIntegrationTests: no raw HTTP
/// endpoint exists for these commands (ConfirmEmail.razor / ResendConfirmation.razor call MediatR
/// directly), so each test uses its own fresh factory and invokes ISender from a plain DI scope.
/// </summary>
public sealed class EmailConfirmationIntegrationTests
{
    [Fact]
    public async Task ConfirmEmail_WithValidToken_MarksEmailConfirmed()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new BeeDayWebApplicationFactory();
        var user = await factory.SeedUnconfirmedUserAsync("confirm-valid@beeday.invalid", "Password123!");
        var token = await factory.IssueEmailConfirmationTokenAsync(user.Id);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(token)), cancellationToken);
        }

        var confirmed = await factory.FindUserAsync("confirm-valid@beeday.invalid");
        Assert.True(confirmed!.IsEmailConfirmed);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_Throws()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new BeeDayWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest("not-a-real-token")), cancellationToken));
    }

    [Fact]
    public async Task ConfirmEmail_WithExpiredToken_Throws()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new BeeDayWebApplicationFactory();
        var user = await factory.SeedUnconfirmedUserAsync("confirm-expired@beeday.invalid", "Password123!");
        var token = await factory.IssueEmailConfirmationTokenAsync(user.Id, expired: true);

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(token)), cancellationToken));

        var user2 = await factory.FindUserAsync("confirm-expired@beeday.invalid");
        Assert.False(user2!.IsEmailConfirmed);
    }

    [Fact]
    public async Task ConfirmEmail_WithAlreadyUsedToken_CannotBeReplayed()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new BeeDayWebApplicationFactory();
        var user = await factory.SeedUnconfirmedUserAsync("confirm-reused@beeday.invalid", "Password123!");
        var token = await factory.IssueEmailConfirmationTokenAsync(user.Id);

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(token)), cancellationToken);

        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(token)), cancellationToken));
    }

    [Fact]
    public async Task ResendEmailConfirmation_ForUnconfirmedUser_RevokesPriorTokenAndIssuesAUsableOne()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();
        var user = await factory.SeedUnconfirmedUserAsync("confirm-resend@beeday.invalid", "Password123!");
        var firstToken = await factory.IssueEmailConfirmationTokenAsync(user.Id);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest("confirm-resend@beeday.invalid")), cancellationToken);
        }

        Assert.Equal(1, factory.CountCapturedEmails());
        var newToken = await factory.TryGetLatestCapturedTokenAsync(cancellationToken);
        Assert.False(string.IsNullOrWhiteSpace(newToken));

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            // The token issued before the resend must now be revoked...
            await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
                sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(firstToken)), cancellationToken));
            // ...while the newly captured one still works.
            await sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(newToken!)), cancellationToken);
        }

        var confirmed = await factory.FindUserAsync("confirm-resend@beeday.invalid");
        Assert.True(confirmed!.IsEmailConfirmed);
    }

    [Fact]
    public async Task ResendEmailConfirmation_ForAlreadyConfirmedUser_CompletesSilentlyWithoutSendingEmail()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();
        await factory.SeedConfirmedUserAsync("confirm-already@beeday.invalid", "Password123!");

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest("confirm-already@beeday.invalid")), cancellationToken);

        Assert.Equal(0, factory.CountCapturedEmails());
    }

    [Fact]
    public async Task ResendEmailConfirmation_ForNonexistentEmail_CompletesSilentlyWithoutSendingEmail()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest("confirm-never-existed@beeday.invalid")), cancellationToken);

        Assert.Equal(0, factory.CountCapturedEmails());
    }

    [Fact]
    public async Task ResendEmailConfirmation_SecondImmediateRequest_IsThrottled()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new EmailCaptureWebApplicationFactory();
        await factory.SeedUnconfirmedUserAsync("confirm-throttled@beeday.invalid", "Password123!");

        await using var scope = factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest("confirm-throttled@beeday.invalid")), cancellationToken);

        // Unlike password reset (which silently no-ops when throttled), resend surfaces the
        // throttle as an explicit error — verified real behavior, not a design decision made here.
        await Assert.ThrowsAsync<InvalidDomainStateException>(() =>
            sender.Send(new ResendEmailConfirmationCommand(new ResendEmailConfirmationRequest("confirm-throttled@beeday.invalid")), cancellationToken));
    }

    [Fact]
    public async Task Login_IsRejectedBeforeConfirmationAndSucceedsAfter()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        await using var factory = new BeeDayWebApplicationFactory();
        var user = await factory.SeedUnconfirmedUserAsync("confirm-login-gate@beeday.invalid", "Password123!");

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var beforeConfirmation = await PostLoginAsync(client, "confirm-login-gate@beeday.invalid", "Password123!", cancellationToken);
        Assert.Contains("error=invalid", beforeConfirmation.Headers.Location!.ToString(), StringComparison.Ordinal);

        var token = await factory.IssueEmailConfirmationTokenAsync(user.Id);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ConfirmEmailCommand(new ConfirmEmailRequest(token)), cancellationToken);
        }

        var afterConfirmation = await PostLoginAsync(client, "confirm-login-gate@beeday.invalid", "Password123!", cancellationToken);
        Assert.False(afterConfirmation.Headers.Location?.ToString().Contains("error=invalid", StringComparison.Ordinal) ?? false);
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
