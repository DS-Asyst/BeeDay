using BeeDay.Application.Features.Ordering.Requests;
using MediatR;

namespace BeeDay.Application.Features.Ordering.Commands;

public sealed record ReorderActivitiesCommand(ReorderActivitiesRequest Request) : IRequest;
