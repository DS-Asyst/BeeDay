using System.Net;
using System.Text;
using System.Text.Json;
using BeeDay.Application.Common.Identity;
using BeeDay.Domain.Enums;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
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

        var message = composer.ComposeEmailConfirmation("player@example.com", "Tiago <Admin>", "a+b/c=", UserLanguage.English);

        Assert.Equal("player@example.com", message.Recipient);
        Assert.Equal("Confirm your beeday email", message.Subject);
        Assert.Contains("https://beeday.example/account/confirm-email?token=a%2Bb%2Fc%3D", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Tiago &lt;Admin&gt;", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Tiago <Admin>", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailComposer_BuildsPasswordResetLink()
    {
        var composer = CreateComposer();

        var message = composer.ComposePasswordReset("player@example.com", "Tiago", "reset-token", UserLanguage.English);

        Assert.Equal("Reset your beeday password", message.Subject);
        Assert.Contains("https://beeday.example/account/reset-password?token=reset-token", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("expires in 1 hour", message.HtmlBody, StringComparison.Ordinal);
    }

    // Epic 26, Sprint 26.6: #5247F9 is the single officially approved beeday Brand Color
    // (docs/design-system/01-foundations.md §2.2; CLAUDE.md §13) — the CTA button must use it, and
    // the stale pre-EPIC-25 purple (#7A4FCB) this template used before must be fully gone.
    [Theory]
    [InlineData("ComposeEmailConfirmation")]
    [InlineData("ComposePasswordReset")]
    public void EmailComposer_UsesTheCurrentBrandColorForTheCallToAction(string method)
    {
        var composer = CreateComposer();

        var message = method == "ComposeEmailConfirmation"
            ? composer.ComposeEmailConfirmation("player@example.com", "Tiago", "token", UserLanguage.English)
            : composer.ComposePasswordReset("player@example.com", "Tiago", "token", UserLanguage.English);

        Assert.Contains("#5247F9", message.HtmlBody, StringComparison.Ordinal);
        Assert.DoesNotContain("#7A4FCB", message.HtmlBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailComposer_IncludesAPlainTextAlternativeWithTheSameLink()
    {
        var composer = CreateComposer();

        var message = composer.ComposeEmailConfirmation("player@example.com", "Tiago", "a+b/c=", UserLanguage.English);

        Assert.NotNull(message.PlainTextBody);
        Assert.DoesNotContain('<', message.PlainTextBody);
        Assert.Contains("https://beeday.example/account/confirm-email?token=a%2Bb%2Fc%3D", message.PlainTextBody, StringComparison.Ordinal);
        Assert.Contains("Tiago", message.PlainTextBody, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailComposer_PasswordResetPlainTextMatchesTheHtmlLinkAndExpiry()
    {
        var composer = CreateComposer();

        var message = composer.ComposePasswordReset("player@example.com", "Tiago", "reset-token", UserLanguage.English);

        Assert.NotNull(message.PlainTextBody);
        Assert.DoesNotContain('<', message.PlainTextBody);
        Assert.Contains("https://beeday.example/account/reset-password?token=reset-token", message.PlainTextBody, StringComparison.Ordinal);
        Assert.Contains("expires in 1 hour", message.PlainTextBody, StringComparison.Ordinal);
    }

    // EPIC 28, Sprint 28.2 (ADR-006): the composer must render each recipient's own persisted
    // UserLanguage, never a shared/ambient culture — these tests exercise both approved languages and
    // both flows to prove that.
    [Theory]
    [InlineData(UserLanguage.English, "Confirm your beeday email", "Hello, Ana!", "en-US")]
    [InlineData(UserLanguage.Portuguese, "Confirme seu e-mail beeday", "Olá, Ana!", "pt-BR")]
    public void EmailComposer_ComposesConfirmationInTheRequestedLanguage(UserLanguage language, string expectedSubject, string expectedGreeting, string expectedHtmlLang)
    {
        var composer = CreateComposer();

        var message = composer.ComposeEmailConfirmation("player@example.com", "Ana", "token", language);

        Assert.Equal(expectedSubject, message.Subject);
        // WebUtility.HtmlEncode converts non-ASCII characters (e.g. the pt-BR "á") to numeric HTML
        // entities, which is correct/safe markup but not a literal substring match — decode first.
        Assert.Contains(expectedGreeting, WebUtility.HtmlDecode(message.HtmlBody), StringComparison.Ordinal);
        Assert.Contains(expectedGreeting, message.PlainTextBody, StringComparison.Ordinal);
        Assert.Contains($"<html lang=\"{expectedHtmlLang}\">", message.HtmlBody, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(UserLanguage.English, "Reset your beeday password", "Hello, Ana!")]
    [InlineData(UserLanguage.Portuguese, "Redefina sua senha beeday", "Olá, Ana!")]
    public void EmailComposer_ComposesPasswordResetInTheRequestedLanguage(UserLanguage language, string expectedSubject, string expectedGreeting)
    {
        var composer = CreateComposer();

        var message = composer.ComposePasswordReset("player@example.com", "Ana", "token", language);

        Assert.Equal(expectedSubject, message.Subject);
        Assert.Contains(expectedGreeting, WebUtility.HtmlDecode(message.HtmlBody), StringComparison.Ordinal);
        Assert.Contains(expectedGreeting, message.PlainTextBody, StringComparison.Ordinal);
    }

    // Every language passed through the boundary must resolve every key the composer needs — a
    // missing pt-BR translation must fail loudly (InvalidOperationException from IdentityEmailComposer),
    // never silently fall back to English content under a pt-BR subject/lang tag.
    [Theory]
    [InlineData(UserLanguage.English)]
    [InlineData(UserLanguage.Portuguese)]
    public void EmailComposer_NeverThrowsForAnyApprovedLanguage(UserLanguage language)
    {
        var composer = CreateComposer();

        var confirmation = Record.Exception(() => composer.ComposeEmailConfirmation("player@example.com", "Ana", "token", language));
        var reset = Record.Exception(() => composer.ComposePasswordReset("player@example.com", "Ana", "token", language));

        Assert.Null(confirmation);
        Assert.Null(reset);
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
            new EmailMessage("player@example.com", "Confirm", "<p>Hello</p>", "Hello"),
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

        Assert.Equal("beeday <noreply@beeday.example>", root.GetProperty("from").GetString());
        Assert.Equal("player@example.com", root.GetProperty("to")[0].GetString());
        Assert.Equal("Confirm", root.GetProperty("subject").GetString());
        Assert.Equal("<p>Hello</p>", root.GetProperty("html").GetString());
        Assert.Equal("Hello", root.GetProperty("text").GetString());
    }

    [Fact]
    public async Task ResendSender_WhenPlainTextBodyIsAbsent_OmitsItAsNull()
    {
        string? payload = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            payload = await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var sender = CreateSender(handler, EnabledOptions());

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Subject", "<p>Body</p>"),
            TestContext.Current.CancellationToken);

        Assert.NotNull(payload);
        using var document = JsonDocument.Parse(payload);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("text").ValueKind);
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

    // Epic 26, Sprint 26.7: observable state model — "provider request attempted" and "provider
    // accepted" (with Resend's own message id, a safe, non-secret correlation identifier) must both
    // be logged; success was previously entirely silent.
    [Fact]
    public async Task ResendSender_OnSuccess_LogsAttemptedThenAcceptedWithProviderMessageId()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\":\"resend-message-id\"}", Encoding.UTF8, "application/json")
        });
        var logger = new RecordingLogger<ResendEmailSender>();
        var sender = CreateSender(handler, EnabledOptions(), logger);

        await sender.SendAsync(
            new EmailMessage("player@example.com", "Confirm", "<p>Hello</p>"),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, logger.Entries.Count);
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("attempted", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information && e.Message.Contains("accepted", StringComparison.OrdinalIgnoreCase) && e.Message.Contains("resend-message-id", StringComparison.Ordinal));
        Assert.All(logger.Entries, e =>
        {
            Assert.DoesNotContain("player@example.com", e.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("re_test", e.Message, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task ResendSender_WhenApiRejectsRequest_LogsRejectionWithStatusCodeButNoRecipientOrApiKey()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent("invalid_api_key")
        });
        var logger = new RecordingLogger<ResendEmailSender>();
        var sender = CreateSender(handler, EnabledOptions(), logger);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sender.SendAsync(
                new EmailMessage("player@example.com", "Subject", "Body"),
                TestContext.Current.CancellationToken));

        var rejection = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.Contains("401", rejection.Message, StringComparison.Ordinal);
        Assert.All(logger.Entries, e =>
        {
            Assert.DoesNotContain("player@example.com", e.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("re_test", e.Message, StringComparison.Ordinal);
        });
    }

    // Distinguishes a transient network failure (no HTTP response ever received) from a provider
    // rejection (a real HTTP response with a non-2xx status) — both must be classified/logged
    // distinctly, per the sprint's required state model, without adding an automatic retry.
    [Fact]
    public async Task ResendSender_WhenNetworkFails_LogsTransientFailureAndRethrowsWithoutRetrying()
    {
        var attempts = 0;
        var handler = new StubHttpMessageHandler((Func<HttpRequestMessage, HttpResponseMessage>)(_ =>
        {
            attempts++;
            throw new HttpRequestException("Simulated DNS/connection failure.");
        }));
        var logger = new RecordingLogger<ResendEmailSender>();
        var sender = CreateSender(handler, EnabledOptions(), logger);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            sender.SendAsync(
                new EmailMessage("player@example.com", "Subject", "Body"),
                TestContext.Current.CancellationToken));

        Assert.Equal(1, attempts);
        var failure = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.NotNull(failure.Exception);
        Assert.DoesNotContain("player@example.com", failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResendSender_WhenCallerCancels_PropagatesWithoutLoggingAFailure()
    {
        var handler = new StubHttpMessageHandler((Func<HttpRequestMessage, HttpResponseMessage>)(_ =>
            throw new NotImplementedException("Should never be reached: cancellation must be observed first.")));
        var logger = new RecordingLogger<ResendEmailSender>();
        var sender = CreateSender(handler, EnabledOptions(), logger);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sender.SendAsync(new EmailMessage("player@example.com", "Subject", "Body"), cts.Token));

        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Error);
    }

    private static IdentityEmailComposer CreateComposer() => new(Options.Create(new IdentityEmailOptions
    {
        PublicBaseUrl = "https://beeday.example",
        ConfirmationPath = "/account/confirm-email",
        PasswordResetPath = "/account/reset-password"
    }));

    private static ResendEmailSender CreateSender(HttpMessageHandler handler, ResendOptions options, ILogger<ResendEmailSender>? logger = null) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") },
            Options.Create(options),
            logger ?? NullLogger<ResendEmailSender>.Instance);

    private static ResendOptions EnabledOptions() => new()
    {
        Enabled = true,
        ApiKey = "re_test",
        FromName = "beeday",
        FromAddress = "noreply@beeday.example"
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
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            return await responder(request);
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, exception, formatter(state, exception)));
        }
    }
}
