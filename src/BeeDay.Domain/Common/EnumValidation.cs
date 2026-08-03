using BeeDay.Domain.Exceptions;

namespace BeeDay.Domain.Common;

internal static class EnumValidation
{
    public static T Defined<T>(T value, string field) where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainValidationException(field, $"Invalid {field} value.");
        }
        return value;
    }
}
