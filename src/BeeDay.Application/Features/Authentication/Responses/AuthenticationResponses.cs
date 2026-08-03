namespace BeeDay.Application.Features.Authentication.Responses;

public sealed record AuthenticatedUserResponse(Guid Id, string Name, string Email, bool HasProfile, bool HasCompletedOnboarding, int SessionVersion);
