using LevelUp.Application.Features.Profiles.Requests;
using MediatR;

namespace LevelUp.Application.Features.Profiles.Commands;

public sealed record SaveProfileCommand(SaveProfileRequest Request) : IRequest;
