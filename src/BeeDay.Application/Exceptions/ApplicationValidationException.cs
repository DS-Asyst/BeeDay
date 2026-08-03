using FluentValidation.Results;

namespace LevelUp.Application.Exceptions;

public sealed class ApplicationValidationException : Exception
{
    public ApplicationValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        ArgumentNullException.ThrowIfNull(failures);

        Errors = failures
            .Where(failure => failure is not null)
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(failure => failure.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);
    }

    public IDictionary<string, string[]> Errors { get; }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return string.Concat(char.ToLowerInvariant(value[0]), value[1..]);
    }
}
