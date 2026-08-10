using BeeDay.Domain.Events;

namespace BeeDay.Application.Tests;

public sealed class DomainEventTests
{
    [Fact]
    public void DomainEvent_CapturesIdentityAndTimestamp()
    {
        var domainEvent = new ApplicationActionDomainEvent("CreateHabitCommand", "CreateHabit");

        Assert.NotEqual(Guid.Empty, domainEvent.EventId);
        Assert.True(domainEvent.OccurredOnUtc <= DateTimeOffset.UtcNow);
    }
}
