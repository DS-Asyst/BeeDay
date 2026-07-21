using LevelUp.Domain.Events;
using MediatR;

namespace LevelUp.Application.Common.Events;

public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
