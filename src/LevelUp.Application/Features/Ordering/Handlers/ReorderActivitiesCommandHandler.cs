using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Ordering.Commands;
using LevelUp.Application.Features.Ordering.Requests;
using MediatR;
namespace LevelUp.Application.Features.Ordering.Handlers;

public sealed class ReorderActivitiesCommandHandler(ILevelUpRepository r, ICurrentUserContext? currentUser = null) : RequestHandlerBase(r), IRequestHandler<ReorderActivitiesCommand>
{
    public Task Handle(ReorderActivitiesCommand c, CancellationToken t) => MutateAsync(d =>
    {
        var userId = CurrentUserGuard.RequireUserId(d, currentUser);
        var x = c.Request;
        switch (x.Collection)
        {
            case ActivityCollection.Habits: d.ReorderHabits(userId, x.OrderedIds); break;
            case ActivityCollection.Tasks: d.ReorderTasks(userId, x.OrderedIds); break;
            case ActivityCollection.Todos: d.ReorderTodos(userId, x.OrderedIds); break;
            case ActivityCollection.Projects: d.ReorderProjects(userId, x.OrderedIds); break;
            default: throw new ArgumentOutOfRangeException(nameof(c), x.Collection, "Unsupported activity collection.");
        }
    }, t);
}
