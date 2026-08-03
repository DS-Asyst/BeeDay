using BeeDay.Application.Features.Dashboard.Responses;

namespace BeeDay.Application.Features.Dashboard.Contracts;

/// <summary>
/// Read-only projection spanning User, Habit, RecurringTask, Project/Todo, and Wallet — the one
/// screen that genuinely needs a slice of every Aggregate at once. Deliberately not a repository:
/// loading each Aggregate Root in full only to flatten it for display is exactly what
/// docs/architecture/02-target-architecture.md §4 asks read services to avoid. Feature-scoped rather
/// than in Common/Contracts because, unlike the Aggregate repositories, nothing outside the
/// Dashboard feature consumes this shape.
/// </summary>
public interface IDashboardReadService
{
    public Task<DashboardResponse> GetAsync(Guid userId, CancellationToken cancellationToken = default);
}
