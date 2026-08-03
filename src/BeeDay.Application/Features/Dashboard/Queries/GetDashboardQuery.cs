using BeeDay.Application.Features.Dashboard.Responses;
using MediatR;
namespace BeeDay.Application.Features.Dashboard.Queries;

/// <summary>
/// The Dashboard page's read query, backed by <c>IDashboardReadService</c> (JSON through Sprint 13.4,
/// SQL Server exclusively from Sprint 14.6's cutover onward). <c>GetLevelUpQuery</c>, the whole-document
/// query this replaced for Dashboard specifically, was removed on that same Sprint once its last
/// consumers (Tutorial/Account/ProfileCreationState) migrated to <c>GetCurrentUserQuery</c> — see
/// docs/architecture/07-persistence-contracts.md.
/// </summary>
public sealed record GetDashboardQuery : IRequest<DashboardResponse>;
