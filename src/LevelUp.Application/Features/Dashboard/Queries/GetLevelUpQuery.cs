using LevelUp.Application.Features.Dashboard.Responses;
using MediatR;
namespace LevelUp.Application.Features.Dashboard.Queries;

public sealed record GetLevelUpQuery : IRequest<GetLevelUpResponse>;
