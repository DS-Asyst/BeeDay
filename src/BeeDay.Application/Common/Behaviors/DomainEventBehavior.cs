using BeeDay.Application.Common.Events;
using BeeDay.Domain.Events;
using MediatR;

namespace BeeDay.Application.Common.Behaviors;

public sealed class DomainEventBehavior<TRequest, TResponse>(IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);
        var requestName = typeof(TRequest).Name;

        if (!requestName.EndsWith("Command", StringComparison.Ordinal))
        {
            return response;
        }

        var category = requestName[..^"Command".Length];
        var entityId = TryGetEntityId(request);
        var domainEvent = new ApplicationActionDomainEvent(requestName, category, entityId);

        await publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        return response;
    }

    private static string? TryGetEntityId(TRequest request)
    {
        var property = typeof(TRequest).GetProperty("Id");
        return property?.GetValue(request)?.ToString();
    }
}
