namespace LevelUp.Application.Features.Authentication.Responses;

public sealed record AuthenticatedUserResponse(Guid Id, string Name, string Email, bool HasCharacter, bool HasCompletedOnboarding);
