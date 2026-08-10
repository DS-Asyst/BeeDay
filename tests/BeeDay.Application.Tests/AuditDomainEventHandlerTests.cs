using BeeDay.Application.Common.Auditing;
using BeeDay.Application.Common.Background;
using BeeDay.Application.Common.Events;
using BeeDay.Domain.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BeeDay.Application.Tests;

public sealed class AuditDomainEventHandlerTests
{
    [Fact]
    public async Task Handle_EnqueuesJournalWriteRatherThanWritingInline()
    {
        var queue = new CapturingBackgroundTaskQueue();
        var journal = new RecordingEventJournal();
        var handler = new AuditDomainEventHandler(queue, journal, NullLogger<AuditDomainEventHandler>.Instance);
        var domainEvent = new ApplicationActionDomainEvent("CreateHabitCommand", "CreateHabit");
        var notification = new DomainEventNotification(domainEvent);

        await handler.Handle(notification, TestContext.Current.CancellationToken);

        Assert.NotNull(queue.QueuedWorkItem);
        Assert.Empty(journal.AppendedEvents);

        await queue.QueuedWorkItem!(TestContext.Current.CancellationToken);

        Assert.Same(domainEvent, Assert.Single(journal.AppendedEvents));
    }

    [Fact]
    public async Task Handle_WhenJournalWriteFails_SwallowsExceptionAndLogsError()
    {
        var queue = new CapturingBackgroundTaskQueue();
        var journal = new ThrowingEventJournal();
        var logger = new RecordingLogger<AuditDomainEventHandler>();
        var handler = new AuditDomainEventHandler(queue, journal, logger);
        var notification = new DomainEventNotification(
            new ApplicationActionDomainEvent("CreateHabitCommand", "CreateHabit"));

        await handler.Handle(notification, TestContext.Current.CancellationToken);

        // The failure happens inside the queued work item, executed here synchronously in the
        // test (there is no real BackgroundTaskWorker) - it must not throw back out, matching the
        // fire-and-forget contract: a broken journal must never fail the Command that triggered it.
        await queue.QueuedWorkItem!(TestContext.Current.CancellationToken);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.IsType<IOException>(entry.Exception);
    }

    private sealed class CapturingBackgroundTaskQueue : IBackgroundTaskQueue
    {
        public Func<CancellationToken, ValueTask>? QueuedWorkItem { get; private set; }

        public ValueTask QueueAsync(
            Func<CancellationToken, ValueTask> workItem,
            CancellationToken cancellationToken = default)
        {
            QueuedWorkItem = workItem;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class RecordingEventJournal : IEventJournal
    {
        public List<IDomainEvent> AppendedEvents { get; } = [];

        public Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            AppendedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingEventJournal : IEventJournal
    {
        public Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) =>
            throw new IOException("Simulated filesystem failure.");
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
