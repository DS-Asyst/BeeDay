namespace LevelUp.Application.Features.Users.Requests;

public sealed record CompleteUserProfileRequest(
    string FullName,
    string Nickname,
    string? Avatar = null);
