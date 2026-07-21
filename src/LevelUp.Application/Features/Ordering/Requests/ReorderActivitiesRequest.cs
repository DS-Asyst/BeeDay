namespace LevelUp.Application.Features.Ordering.Requests;

public sealed record ReorderActivitiesRequest(
    ActivityCollection Collection,
    IReadOnlyList<Guid> OrderedIds);
