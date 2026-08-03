using BeeDay.Domain.Enums;

namespace BeeDay.Application.Features.Users.Requests;

public sealed record UpdateUserPreferencesRequest(UserLanguage Language, UserTheme Theme);
