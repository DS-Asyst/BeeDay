using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Ordering.Commands;
using LevelUp.Application.Features.Ordering.Requests;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Ordering.Handlers;

public sealed class ReorderActivitiesCommandHandler(
    IHabitRepository habitRepository,
    IRecurringTaskRepository taskRepository,
    IProjectRepository projectRepository,
    ICurrentUserContext currentUser) : IRequestHandler<ReorderActivitiesCommand>
{
    public async Task Handle(ReorderActivitiesCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;

        switch (request.Collection)
        {
            case ActivityCollection.Habits:
                {
                    var owned = await habitRepository.ListAsync(userId, cancellationToken);
                    EnsureOwned(request.OrderedIds, owned.Select(habit => habit.Id));
                    await habitRepository.ReorderAsync(userId, request.OrderedIds, cancellationToken);
                    break;
                }
            case ActivityCollection.Tasks:
                {
                    var owned = await taskRepository.ListAsync(userId, cancellationToken);
                    EnsureOwned(request.OrderedIds, owned.Select(task => task.Id));
                    await taskRepository.ReorderAsync(userId, request.OrderedIds, cancellationToken);
                    break;
                }
            case ActivityCollection.Projects:
                {
                    var owned = await projectRepository.ListAsync(userId, cancellationToken);
                    EnsureOwned(request.OrderedIds, owned.Select(project => project.Id));
                    await projectRepository.ReorderAsync(userId, request.OrderedIds, cancellationToken);
                    break;
                }
            case ActivityCollection.Todos:
                {
                    // Unlike Habits/Tasks/Projects (one flat owned list), Todos are scoped per Project, so a
                    // single reordered batch can span multiple Projects — grouped here exactly like
                    // LevelUpData.ReorderTodos used to, then persisted once per Project via ReorderTodosAsync.
                    if (request.OrderedIds.Count < 2)
                    {
                        break;
                    }

                    var projects = await projectRepository.ListAsync(userId, cancellationToken);
                    var groups = request.OrderedIds
                        .Select(id => (Id: id, Project: projects.FirstOrDefault(project => project.Todos.Any(todo => todo.Id == id))
                            ?? throw new InvalidDomainStateException($"To-Do '{id}' was not found for the authenticated User.")))
                        .GroupBy(entry => entry.Project.Id);

                    foreach (var group in groups)
                    {
                        await projectRepository.ReorderTodosAsync(userId, group.Key, group.Select(entry => entry.Id).ToList(), cancellationToken);
                    }

                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(command), request.Collection, "Unsupported activity collection.");
        }
    }

    /// <summary>
    /// EfXxxRepository.ReorderAsync (Sprint 14.4) silently ignores any id outside the User's own rows —
    /// it was never asked to replicate LevelUpData.ReorderHabits/Tasks/Projects's stricter behavior of
    /// throwing when the request references another User's activity. Validating here, before delegating,
    /// preserves that behavior without changing the already-tested repository method.
    /// </summary>
    private static void EnsureOwned(IReadOnlyList<Guid> orderedIds, IEnumerable<Guid> ownedIds)
    {
        if (orderedIds.Count < 2)
        {
            return;
        }

        var owned = ownedIds.ToHashSet();
        if (orderedIds.Any(id => !owned.Contains(id)))
        {
            throw new InvalidDomainStateException("The reorder request contains an activity owned by another User.");
        }

        if (orderedIds.Distinct().Count() != orderedIds.Count)
        {
            throw new ArgumentException("The reorder request contains duplicate identifiers.", nameof(orderedIds));
        }
    }
}
