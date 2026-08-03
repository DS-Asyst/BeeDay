using BeeDay.Application.Features.Identity.Requests;
using MediatR;

namespace BeeDay.Application.Features.Identity.Commands;

public sealed record ConfirmEmailCommand(ConfirmEmailRequest Request) : IRequest;
public sealed record ResendEmailConfirmationCommand(ResendEmailConfirmationRequest Request) : IRequest;
public sealed record RequestPasswordResetCommand(RequestPasswordResetRequest Request) : IRequest;
public sealed record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest;
