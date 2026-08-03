using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Abstractions;

public abstract class Entity
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    protected void EnsureIdentity()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidDomainStateException("Entity identifier cannot be empty.");
        }
    }
}
