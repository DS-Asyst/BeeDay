using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Dashboard.Contracts;
using LevelUp.Application.Features.Dashboard.Queries;
using LevelUp.Application.Features.Dashboard.Responses;
using MediatR;

namespace LevelUp.Application.Features.Dashboard.Handlers;

public sealed class GetDashboardQueryHandler(IDashboardReadService dashboardReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    public Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken) =>
        dashboardReadService.GetAsync(CurrentUserGuard.RequireUserId(currentUser), cancellationToken);
}
