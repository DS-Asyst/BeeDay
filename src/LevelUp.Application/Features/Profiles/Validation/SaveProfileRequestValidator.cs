using FluentValidation;
using LevelUp.Application.Features.Profiles.Requests;
using LevelUp.Domain.ValueObjects;

namespace LevelUp.Application.Features.Profiles.Validation;

public sealed class SaveProfileRequestValidator : AbstractValidator<SaveProfileRequest>
{
    public SaveProfileRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty().WithMessage("Character name is required.")
            .MaximumLength(ProfileName.MaximumLength)
            .WithMessage($"Character name cannot exceed {ProfileName.MaximumLength} characters.");

        RuleFor(request => request.Nickname)
            .NotEmpty().WithMessage("Nickname is required.")
            .Must(nickname => NormalizeNickname(nickname).Length is >= 3 and <= 24)
            .WithMessage("Nickname must contain between 3 and 24 characters.")
            .Matches(@"^@?[A-Za-z0-9._-]+$")
            .WithMessage("Use only letters, numbers, dots, underscores or hyphens.")
            .MaximumLength(ProfileNickname.MaximumLength);

        RuleFor(request => request.CharacterClass).IsInEnum();
    }

    private static string NormalizeNickname(string? nickname) =>
        (nickname ?? string.Empty).Trim().TrimStart('@');
}
