using BeeDay.Domain.Events;
using MediatR;

namespace BeeDay.Application.Common.Events;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
