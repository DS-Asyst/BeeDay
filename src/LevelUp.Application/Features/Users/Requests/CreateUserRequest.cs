namespace LevelUp.Application.Features.Users.Requests;

public sealed record CreateUserRequest(string Name, string Email, string? PasswordHash = null);
