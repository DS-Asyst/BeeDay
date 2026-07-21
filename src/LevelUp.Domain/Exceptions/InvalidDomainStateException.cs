namespace LevelUp.Domain.Exceptions;

public sealed class InvalidDomainStateException : DomainException
{
    public InvalidDomainStateException(string message) : base(message)
    {
    }
}
