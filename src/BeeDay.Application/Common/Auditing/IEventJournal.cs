using BeeDay.Domain.Events;

namespace BeeDay.Application.Common.Auditing;

public interface IEventJournal
{
    public Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
