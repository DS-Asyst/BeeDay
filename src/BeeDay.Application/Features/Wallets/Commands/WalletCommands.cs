using BeeDay.Application.Features.Wallets.Requests;
using MediatR;

namespace BeeDay.Application.Features.Wallets.Commands;

public sealed record EnsureCurrentWalletCommand : IRequest<Guid>;
public sealed record CreateTransactionCommand(SaveTransactionRequest Request) : IRequest<Guid>;
public sealed record UpdateTransactionCommand(Guid Id, SaveTransactionRequest Request) : IRequest;
public sealed record DeleteTransactionCommand(Guid Id) : IRequest;
public sealed record CreateWalletTagCommand(SaveWalletTagRequest Request) : IRequest<Guid>;
public sealed record UpdateWalletTagCommand(Guid Id, SaveWalletTagRequest Request) : IRequest;
public sealed record DeleteWalletTagCommand(Guid Id) : IRequest;
