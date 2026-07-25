using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Features.Inventory.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Inventory.Handlers;

public sealed class EnsureCurrentWalletCommandHandler(ILevelUpRepository repository)
    : IRequestHandler<EnsureCurrentWalletCommand, Guid>
{
    public async Task<Guid> Handle(EnsureCurrentWalletCommand request, CancellationToken cancellationToken)
    {
        var walletId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = RequireCurrentUser(data);
            var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
            if (wallet is null)
            {
                wallet = Wallet.Create(user.Id);
                data.AddWallet(wallet);
            }
            walletId = wallet.Id;
        }, cancellationToken);
        return walletId;
    }

    internal static User RequireCurrentUser(LevelUpData data) =>
        data.CurrentUser ?? throw new InvalidDomainStateException("Current User was not found.");

    internal static Wallet RequireCurrentWallet(LevelUpData data)
    {
        var user = RequireCurrentUser(data);
        return data.Wallets.FirstOrDefault(wallet => wallet.UserId == user.Id)
            ?? throw new InvalidDomainStateException("Current User does not have a Wallet.");
    }

    internal static InventoryTag RequireOwnedTag(LevelUpData data, Guid tagId, Guid userId)
    {
        var tag = data.FindInventoryTag(tagId);
        if (tag.UserId != userId)
        {
            throw new InvalidDomainStateException("Inventory tag does not belong to the current User.");
        }
        return tag;
    }

    internal static Transaction RequireOwnedTransaction(LevelUpData data, Guid transactionId, Wallet wallet)
    {
        var transaction = data.FindTransaction(transactionId);
        if (transaction.WalletId != wallet.Id)
        {
            throw new InvalidDomainStateException("Transaction does not belong to the current User.");
        }
        return transaction;
    }
}

public sealed class CreateTransactionCommandHandler(ILevelUpRepository repository)
    : IRequestHandler<CreateTransactionCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var transactionId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data);
            var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
            if (wallet is null)
            {
                wallet = Wallet.Create(user.Id);
                data.AddWallet(wallet);
            }

            var request = command.Request;
            if (request.InventoryTagId is Guid tagId)
            {
                EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, tagId, user.Id);
            }

            var transaction = Transaction.Create(
                wallet.Id,
                request.Description,
                request.Amount,
                request.Type,
                request.TransactionDate,
                request.InventoryTagId,
                request.Notes);
            data.AddTransaction(transaction);
            transactionId = transaction.Id;
        }, cancellationToken);
        return transactionId;
    }
}

public sealed class UpdateTransactionCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateTransactionCommand>
{
    public Task Handle(UpdateTransactionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data);
            var wallet = EnsureCurrentWalletCommandHandler.RequireCurrentWallet(data);
            var transaction = EnsureCurrentWalletCommandHandler.RequireOwnedTransaction(data, command.Id, wallet);
            var request = command.Request;
            if (request.InventoryTagId is Guid tagId)
            {
                EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, tagId, user.Id);
            }

            transaction.Update(
                request.Description,
                request.Amount,
                request.Type,
                request.TransactionDate,
                request.InventoryTagId,
                request.Notes);
            wallet.Touch();
        }, cancellationToken);
}

public sealed class DeleteTransactionCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<DeleteTransactionCommand>
{
    public Task Handle(DeleteTransactionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var wallet = EnsureCurrentWalletCommandHandler.RequireCurrentWallet(data);
            var transaction = EnsureCurrentWalletCommandHandler.RequireOwnedTransaction(data, command.Id, wallet);
            data.Transactions.Remove(transaction);
            wallet.Touch();
        }, cancellationToken);
}

public sealed class CreateInventoryTagCommandHandler(ILevelUpRepository repository)
    : IRequestHandler<CreateInventoryTagCommand, Guid>
{
    public async Task<Guid> Handle(CreateInventoryTagCommand command, CancellationToken cancellationToken)
    {
        var tagId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data);
            var request = command.Request;
            var tag = InventoryTag.Create(user.Id, request.Name, request.Color);
            data.AddInventoryTag(tag);
            tagId = tag.Id;
        }, cancellationToken);
        return tagId;
    }
}

public sealed class UpdateInventoryTagCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<UpdateInventoryTagCommand>
{
    public Task Handle(UpdateInventoryTagCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data);
            var tag = EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, command.Id, user.Id);
            var normalizedName = string.Join(' ', command.Request.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (data.InventoryTags.Any(candidate => candidate.Id != tag.Id && candidate.UserId == user.Id &&
                string.Equals(candidate.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDomainStateException($"Inventory tag '{normalizedName}' already exists for this User.");
            }
            tag.Update(command.Request.Name, command.Request.Color);
        }, cancellationToken);
}

public sealed class DeleteInventoryTagCommandHandler(ILevelUpRepository repository)
    : RequestHandlerBase(repository), IRequestHandler<DeleteInventoryTagCommand>
{
    public Task Handle(DeleteInventoryTagCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data);
            EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, command.Id, user.Id);
            data.RemoveInventoryTag(command.Id);
            data.Wallets.FirstOrDefault(wallet => wallet.UserId == user.Id)?.Touch();
        }, cancellationToken);
}
