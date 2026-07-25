using LevelUp.Application.Features.Inventory.Requests;
using MediatR;

namespace LevelUp.Application.Features.Inventory.Commands;

public sealed record EnsureCurrentWalletCommand : IRequest<Guid>;
public sealed record CreateTransactionCommand(SaveTransactionRequest Request) : IRequest<Guid>;
public sealed record UpdateTransactionCommand(Guid Id, SaveTransactionRequest Request) : IRequest;
public sealed record DeleteTransactionCommand(Guid Id) : IRequest;
public sealed record CreateInventoryTagCommand(SaveInventoryTagRequest Request) : IRequest<Guid>;
public sealed record UpdateInventoryTagCommand(Guid Id, SaveInventoryTagRequest Request) : IRequest;
public sealed record DeleteInventoryTagCommand(Guid Id) : IRequest;
