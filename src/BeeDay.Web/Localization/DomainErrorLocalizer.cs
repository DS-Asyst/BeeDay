using System.Text.RegularExpressions;
using BeeDay.Application.Exceptions;
using BeeDay.Domain.Exceptions;
using BeeDay.Web.Resources;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Localization;

/// <summary>
/// Maps a caught Domain/Application exception to a localized, user-facing message, so pages never
/// render <c>exception.Message</c> (always raw English, produced by layers that stay independent of
/// localization/UI per the Clean Architecture boundary) directly to the user. Domain and Application
/// keep throwing plain English messages; only this Web-layer translator interprets them, matching on
/// exception type and known message text — the same approach ConfirmEmail.razor already used ad hoc
/// for token-state classification, generalized here for reuse across Account/ProfileCreation/Identity
/// pages. Unrecognized messages fall back to a generic localized message instead of ever leaking the
/// raw Domain/Application text.
/// </summary>
public static class DomainErrorLocalizer
{
    private static readonly Regex WaitSecondsPattern = new(@"\d+", RegexOptions.Compiled);

    public static string Translate(Exception exception, IStringLocalizer<SharedResources> localizer) => exception switch
    {
        InvalidDomainStateException invalidState => TranslateInvalidState(invalidState.Message, localizer),
        DomainValidationException validation => TranslateValidation(validation, localizer),
        ApplicationValidationException => localizer["DomainErrorGenericValidation"],
        _ => localizer["DomainErrorGeneric"]
    };

    private static string TranslateInvalidState(string message, IStringLocalizer<SharedResources> localizer)
    {
        if (message.Contains("already registered", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorEmailAlreadyRegistered"];
        }

        if (message.Contains("already in use", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorNicknameAlreadyInUse"];
        }

        if (message.Contains("current password is incorrect", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorCurrentPasswordIncorrect"];
        }

        if (message.Contains("must be different from the current password", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorNewPasswordSameAsCurrent"];
        }

        if (message.Contains("password reset token", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorPasswordResetTokenInvalid"];
        }

        if (message.Contains("complete their profile once", StringComparison.OrdinalIgnoreCase))
        {
            return localizer["DomainErrorProfileAlreadyCompleted"];
        }

        if (message.Contains("please wait", StringComparison.OrdinalIgnoreCase))
        {
            var seconds = WaitSecondsPattern.Match(message) is { Success: true } match ? match.Value : "60";
            return localizer["DomainErrorWaitBeforeResend", seconds];
        }

        return localizer["DomainErrorGeneric"];
    }

    private static string TranslateValidation(DomainValidationException validation, IStringLocalizer<SharedResources> localizer) =>
        validation.Field.ToLowerInvariant() switch
        {
            "email" => localizer["DomainErrorInvalidEmail"],
            "nickname" => localizer["DomainErrorInvalidNickname"],
            "name" => localizer["DomainErrorInvalidName"],
            _ => localizer["DomainErrorGenericValidation"]
        };
}
