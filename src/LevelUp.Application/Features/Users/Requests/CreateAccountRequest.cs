namespace LevelUp.Application.Features.Users.Requests;

public sealed record CreateAccountRequest(
    string Name,
    string Email,
    string Password,
    string Nickname,
    string? Avatar = null);
