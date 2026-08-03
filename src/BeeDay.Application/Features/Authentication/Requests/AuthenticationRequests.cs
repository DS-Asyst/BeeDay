namespace BeeDay.Application.Features.Authentication.Requests;

public sealed record AuthenticateUserRequest(string Email, string Password);
