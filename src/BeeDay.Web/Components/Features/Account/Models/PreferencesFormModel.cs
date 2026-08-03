using BeeDay.Domain.Enums;

namespace BeeDay.Web.Components.Features.Account.Models;

public sealed class PreferencesFormModel
{
    public UserLanguage Language { get; set; } = UserLanguage.English;
    public UserTheme Theme { get; set; } = UserTheme.System;
}
