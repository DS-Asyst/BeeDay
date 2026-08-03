using System.Net;
using System.Text;
using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class IdentityInfrastructureTests
{
    [Fact]
    public void SecureTokenService_GeneratesUniqueUrlSafeTokens()
    {
        var service = new SecureUserTokenService();

        var first = service.GenerateToken();
        var second = service.GenerateToken();

        Assert.NotEqual(first, second);
        Assert.Equal(43, first.Length);
        Assert.DoesNotContain('+', first);
        Assert.DoesNotContain('/', first);
        Assert.DoesNotContain('=', first);
    }

    [Fact]
    public void SecureTokenService_HashesDeterministicallyWithoutReturningRawToken()
    {
        var service = new SecureUserTokenService();

        var first = service.HashToken("raw-token");
        var second = service.HashToken("raw-token");

        Assert.Equal(first, second);
        Assert.NotEqual("raw-token", first);
        Assert.Equal(64, first.Length);
    }

    [Fact]
    public void EmailComposer_BuildsEncodedConfirmationLinkAndHtml()
    {
        var composer = CreateComposer();

        var message = composer.ComposeEmailConfirmation("player@example.com", "Tiago <Admin>", "a+b/c=");

        Assert.Equal("player@example.com", message.Recipient);
        Assert.Equal("Confirm your LevelUp email", message.Subject);
        Assert.Contains("https://levelup.example/account/confirm-email?token=a%2Bb%2Fc%3D", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Tiago &lt;Admin&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Tiago <Admin>", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailComposer_BuildsPasswordResetLink()
    {
        var composer = CreateComposer();

        var message = composer.ComposePasswordReset("player@example.com", "Tiago", "reset-token");

        Assert.Equal("Reset your LevelUp password", message.Subject);
        Assert.Contains("https://levelup.example/account/reset-password?token=reset-token", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("expires in 1 hour", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ResendSender_WhenDisabled_DoesNotCallApi()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var sender = CreateSender(handler, new ResendOptions { Enabled = false });

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>"),
            TestContext.Current.CancellationToken);

        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task ResendSender_SendsExpectedAuthenticatedRequest()
    {
        HttpRequestMessage? captured = null;
        string? payload = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            captured = request;
            payload = await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"email-id\"}", Encoding.UTF8, "application/json")
            };
        });
        var sender = CreateSender(handler, EnabledOptions());

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Confirm", "<p>Hello</p>"),
            TestContext.Current.CancellationToken);

        Assert.NotNull(captured);
        Assert.Equal(HttpMethod.Post, captured.Method);
        Assert.Equal("https://api.resend.com/emails", captured.RequestUri!.AbsoluteUri);
        Assert.Equal("Bearer", captured.Headers.Authorization!.Scheme);
        Assert.Equal("re_test", captured.Headers.Authorization.Parameter);
        Assert.True(captured.Headers.Contains("User-Agent"));
        Assert.True(captured.Headers.Contains("Idempotency-Key"));
        Assert.NotNull(payload);
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        Assert.Equal("LevelUp <noreply@levelup.example>", root.GetProperty("from").GetString());
        Assert.Equal("player@example.com", root.GetProperty("to")[0].GetString());
        Assert.Equal("Confirm", root.GetProperty("subject").GetString());
        Assert.Equal("<p>Hello</p>", root.GetProperty("html").GetString());
    }

    [Fact]
    public async Task ResendSender_WhenApiRejectsRequest_ThrowsWithoutExposingApiKey()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("invalid_api_key")
        });
        var sender = CreateSender(handler, EnabledOptions());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            sender.SendAsync(
                new EmailMessage("player@example.com", "Subject", "Body"),
                TestContext.Current.CancellationToken));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.DoesNotContain("re_test", exception.Message, StringComparison.Ordinal);
    }

    private static IdentityEmailComposer CreateComposer() => new(Options.Create(new IdentityEmailOptions
    {
        PublicBaseUrl = "https://levelup.example",
        ConfirmationPath = "/account/confirm-email",
        PasswordResetPath = "/account/reset-password"
    }));

    private static ResendEmailSender CreateSender(HttpMessageHandler handler, ResendOptions options) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") },
            Options.Create(options),
            NullLogger<ResendEmailSender>.Instance);

    private static ResendOptions EnabledOptions() => new()
    {
        Enabled = true,
        ApiKey = "re_test",
        FromName = "LevelUp",
        FromAddress = "noreply@levelup.example"
    };

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
            : this(request => Task.FromResult(responder(request)))
        {
        }

        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return await responder(request);
        }
    }
}
