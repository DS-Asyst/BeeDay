using LevelUp.Application.Features.Dashboard.Responses;
using MediatR;
namespace LevelUp.Application.Features.Dashboard.Queries;

/// <summary>
/// Replaces <see cref="GetLevelUpQuery"/> for the Dashboard page only. <c>GetLevelUpQuery</c> stays
/// alive, unmodified, for Tutorial/Account/ProfileCreationState until they migrate in a later Sprint
/// 13.4 lot — see docs/architecture/07-persistence-contracts.md.
/// </summary>
public sealed record GetDashboardQuery : IRequest<DashboardResponse>;
