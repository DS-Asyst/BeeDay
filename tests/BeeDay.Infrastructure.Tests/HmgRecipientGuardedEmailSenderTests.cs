using BeeDay.Application.Common.Identity;
using BeeDay.Infrastructure.Configuration;
using BeeDay.Infrastructure.Diagnostics;
using BeeDay.Infrastructure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace BeeDay.Infrastructure.Tests;

public sealed class HmgRecipientGuardedEmailSenderTests
{
    private const string AllowedRecipient = "owner@beeday.example";
    private const string BlockedRecipient = "someone-else@example.com";

    [Fact]
    public async Task SendAsync_WhenRecipientIsAllowed_InvokesInnerSenderWithPrefixedSubject()
    {
        var inner = new RecordingEmailSender();
        var sender = CreateSender(inner, enabled: true, allowedRecipients: [AllowedRecipient], subjectPrefix: "[HMG] ");

        await sender.SendAsync(new EmailMessage(AllowedRecipient, "Confirm your beeday email", "<p>Hello</p>"), TestContext.Current.CancellationToken);

        var sent = Assert.Single(inner.SentMessages);
        Assert.Equal(AllowedRecipient, sent.Recipient);
        Assert.Equal("[HMG] Confirm your beeday email", sent.Subject);
    }

    [Fact]
    public async Task SendAsync_WhenRecipientIsBlocked_DoesNotInvokeInnerSender()
    {
        var inner = new RecordingEmailSender();
        var sender = CreateSender(inner, enabled: true, allowedRecipients: [AllowedRecipient]);

        await sender.SendAsync(new EmailMessage(BlockedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Empty(inner.SentMessages);
    }

    [Fact]
    public async Task SendAsync_RecipientComparisonIsCaseInsensitiveAndTrimmed()
    {
        var inner = new RecordingEmailSender();
        var sender = CreateSender(inner, enabled: true, allowedRecipients: [" Owner@BeeDay.Example "]);

        await sender.SendAsync(new EmailMessage("owner@beeday.example", "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Single(inner.SentMessages);
    }

    [Fact]
    public async Task SendAsync_WhenGuardDisabled_InvokesInnerSenderWithSubjectUnchanged()
    {
        var inner = new RecordingEmailSender();
        var sender = CreateSender(inner, enabled: false, allowedRecipients: []);

        await sender.SendAsync(new EmailMessage(BlockedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        var sent = Assert.Single(inner.SentMessages);
        Assert.Equal(BlockedRecipient, sent.Recipient);
        Assert.Equal("Subject", sent.Subject);
    }

    [Fact]
    public async Task SendAsync_DoesNotDoublePrefixAnAlreadyPrefixedSubject()
    {
        var inner = new RecordingEmailSender();
        var sender = CreateSender(inner, enabled: true, allowedRecipients: [AllowedRecipient], subjectPrefix: "[HMG] ");

        await sender.SendAsync(new EmailMessage(AllowedRecipient, "[HMG] Already prefixed", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Equal("[HMG] Already prefixed", Assert.Single(inner.SentMessages).Subject);
    }

    [Fact]
    public async Task SendAsync_NeverLogsTheRawRecipientAddress_ForAllowedOrBlockedRecipients()
    {
        var inner = new RecordingEmailSender();
        var logger = new RecordingLogger<HmgRecipientGuardedEmailSender>();
        var sender = new HmgRecipientGuardedEmailSender(
            inner,
            Options.Create(new HmgRecipientGuardOptions { Enabled = true, AllowedRecipients = [AllowedRecipient] }),
            logger);

        await sender.SendAsync(new EmailMessage(AllowedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);
        await sender.SendAsync(new EmailMessage(BlockedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Equal(2, logger.Entries.Count);
        Assert.All(logger.Entries, entry =>
        {
            Assert.DoesNotContain(AllowedRecipient, entry.Message, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(BlockedRecipient, entry.Message, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static HmgRecipientGuardedEmailSender CreateSender(
        IEmailSender inner,
        bool enabled,
        IReadOnlyList<string> allowedRecipients,
        string subjectPrefix = "[HMG] ") =>
        new(
            inner,
            Options.Create(new HmgRecipientGuardOptions { Enabled = enabled, AllowedRecipients = allowedRecipients, SubjectPrefix = subjectPrefix }),
            new RecordingLogger<HmgRecipientGuardedEmailSender>());

    private sealed class RecordingEmailSender : IEmailSender
    {
        public List<EmailMessage> SentMessages { get; } = [];

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            SentMessages.Add(message);
            return Task.CompletedTask;
        }
    }

    // EPIC 28, Sprint 28.7: an operator must be able to tell "allowed" from "blocked" by a stable
    // EventId, not just by parsing message text — this proves the two log lines carry the right ones.
    [Fact]
    public async Task SendAsync_LogsTheCorrectEventIdForAllowedAndBlockedRecipients()
    {
        var inner = new RecordingEmailSender();
        var logger = new RecordingLogger<HmgRecipientGuardedEmailSender>();
        var sender = new HmgRecipientGuardedEmailSender(
            inner,
            Options.Create(new HmgRecipientGuardOptions { Enabled = true, AllowedRecipients = [AllowedRecipient] }),
            logger);

        await sender.SendAsync(new EmailMessage(AllowedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);
        await sender.SendAsync(new EmailMessage(BlockedRecipient, "Subject", "<p>Body</p>"), TestContext.Current.CancellationToken);

        Assert.Contains(logger.Entries, e => e.EventId.Id == EmailEventIds.GuardAllowed.Id);
        Assert.Contains(logger.Entries, e => e.EventId.Id == EmailEventIds.GuardBlocked.Id);
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, EventId EventId, Exception? Exception, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add((logLevel, eventId, exception, formatter(state, exception)));
        }
    }
}
