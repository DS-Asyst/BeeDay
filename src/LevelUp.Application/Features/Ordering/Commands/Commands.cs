using LevelUp.Application.Features.Ordering.Requests;
using MediatR;

namespace LevelUp.Application.Features.Ordering.Commands;

public sealed record ReorderActivitiesCommand(ReorderActivitiesRequest Request) : IRequest;
