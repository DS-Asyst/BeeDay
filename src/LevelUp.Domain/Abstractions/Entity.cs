using System.Text.Json.Serialization;
using LevelUp.Domain.Exceptions;

namespace LevelUp.Domain.Abstractions;

public abstract class Entity
{
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    protected void EnsureIdentity()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidDomainStateException("Entity identifier cannot be empty.");
        }
    }
}
