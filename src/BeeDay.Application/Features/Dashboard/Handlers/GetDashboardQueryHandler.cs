using BeeDay.Application.Common.Security;
using BeeDay.Application.Features.Dashboard.Contracts;
using BeeDay.Application.Features.Dashboard.Queries;
using BeeDay.Application.Features.Dashboard.Responses;
using MediatR;

namespace BeeDay.Application.Features.Dashboard.Handlers;

public sealed class GetDashboardQueryHandler(IDashboardReadService dashboardReadService, ICurrentUserContext currentUser)
    : IRequestHandler<GetDashboardQuery, DashboardResponse>
{
    public Task<DashboardResponse> Handle(GetDashboardQuery request, CancellationToken cancellationToken) =>
        dashboardReadService.GetAsync(CurrentUserGuard.RequireUserId(currentUser), cancellationToken);
}
