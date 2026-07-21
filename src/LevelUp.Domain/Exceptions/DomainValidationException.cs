namespace LevelUp.Domain.Exceptions;

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string field, string message) : base(message)
    {
        Field = field;
    }

    public string Field { get; }
}
