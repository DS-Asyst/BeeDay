using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Exceptions;
using LevelUp.Domain.Entities;

namespace LevelUp.Application.Common.Messaging;

public abstract class RequestHandlerBase(ILevelUpRepository repository)
{
    protected Task MutateAsync(Action<LevelUpData> mutation, CancellationToken cancellationToken) =>
        repository.UpdateAsync(mutation, cancellationToken);

    protected Task DeleteAsync<T>(Func<LevelUpData, List<T>> selector, Guid id, CancellationToken cancellationToken)
        where T : Activity =>
        MutateAsync(data =>
        {
            var items = selector(data);
            items.Remove(Find(items, id));
        }, cancellationToken);

    protected static T Find<T>(IEnumerable<T> items, Guid id) where T : Activity =>
        items.FirstOrDefault(item => item.Id == id) ?? throw new ActivityNotFoundException(id);
}
