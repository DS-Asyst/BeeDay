using LevelUp.Application.Common.Security;

namespace LevelUp.Application.Tests;

/// <summary>
/// Shared <see cref="ICurrentUserContext"/> fake — consolidates what used to be nine identical
/// private nested records named <c>UserContext</c> across this project (Sprint 13.6).
/// </summary>
internal sealed record FakeCurrentUserContext(Guid? UserId) : ICurrentUserContext;
