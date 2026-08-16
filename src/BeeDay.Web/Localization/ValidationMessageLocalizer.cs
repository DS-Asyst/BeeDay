using BeeDay.Web.Components.DesignSystem;
using Microsoft.Extensions.Localization;

namespace BeeDay.Web.Localization;

/// <summary>
/// Maps a DataAnnotations-produced validation message (always plain English — attribute
/// <c>ErrorMessage</c> values are compile-time constants) to its localized equivalent. Used by
/// <c>BeeDayValidationMessage</c>, the single chokepoint every Design System form input
/// (BeeDayInput/BeeDayTextArea/BeeDaySelect/BeeDayCheckbox/BeeDayDateInput) already renders
/// validation feedback through, so fixing localization here covers every editor form in the app
/// without touching the *EditorModel.cs classes or their DataAnnotations attributes. An unrecognized
/// message (a future validator added without updating this map) is shown as-is rather than hidden,
/// so a gap stays visible instead of failing silently.
/// </summary>
public static class ValidationMessageLocalizer
{
    private static readonly Dictionary<string, string> Keys = new(StringComparer.Ordinal)
    {
        ["Current password is required."] = "ValidationCurrentPasswordRequired",
        ["New password is required."] = "ValidationNewPasswordRequired",
        ["Password must contain between 8 and 128 characters."] = "ValidationPasswordLength",
        ["Password must contain at least one letter and one number."] = "ValidationPasswordComplexity",
        ["Confirm the new password."] = "ValidationConfirmNewPasswordRequired",
        ["Passwords do not match."] = "ValidationPasswordsMismatch",
        ["Name is required."] = "ValidationNameRequired",
        ["Name must contain between 2 and 100 characters."] = "ValidationNameLength",
        ["Email is required."] = "ValidationEmailRequired",
        ["Enter a valid email address."] = "ValidationEmailFormat",
        ["Name cannot exceed 100 characters."] = "ValidationNameMaxLength",
        ["Notes cannot exceed 500 characters."] = "ValidationNotesMaxLength",
        ["Use a valid hexadecimal color."] = "ValidationHexColorFormat",
        ["Title is required."] = "ValidationTitleRequired",
        ["Title cannot exceed 100 characters."] = "ValidationTitleMaxLength",
        ["Project is required."] = "ValidationProjectRequired"
    };

    public static string Translate(string message, IStringLocalizer<DesignSystemResources> localizer) =>
        Keys.TryGetValue(message, out var key) ? localizer[key] : message;
}
