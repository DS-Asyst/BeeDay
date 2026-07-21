using LevelUp.Domain.Events;

namespace LevelUp.Application.Common.Auditing;

public interface IEventJournal
{
    public Task AppendAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
}
