using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Messaging;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Wallets.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Wallets.Handlers;

public sealed class EnsureCurrentWalletCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : IRequestHandler<EnsureCurrentWalletCommand, Guid>
{
    public async Task<Guid> Handle(EnsureCurrentWalletCommand request, CancellationToken cancellationToken)
    {
        var walletId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));
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

    internal static User RequireCurrentUser(LevelUpData data, ICurrentUserContext? currentUser) =>
        data.FindUser(CurrentUserGuard.RequireUserId(data, currentUser));

    internal static Wallet RequireCurrentWallet(LevelUpData data, ICurrentUserContext? currentUser)
    {
        var user = RequireCurrentUser(data, currentUser);
        return data.Wallets.FirstOrDefault(wallet => wallet.UserId == user.Id)
            ?? throw new InvalidDomainStateException("Current User does not have a Wallet.");
    }

    internal static WalletTag RequireOwnedTag(LevelUpData data, Guid tagId, Guid userId)
    {
        var tag = data.FindWalletTag(tagId);
        if (tag.UserId != userId)
        {
            throw new InvalidDomainStateException("Wallet tag does not belong to the current User.");
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

public sealed class CreateTransactionCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : IRequestHandler<CreateTransactionCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var transactionId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data, currentUser);
            var wallet = data.Wallets.FirstOrDefault(candidate => candidate.UserId == user.Id);
            if (wallet is null)
            {
                wallet = Wallet.Create(user.Id);
                data.AddWallet(wallet);
            }

            var request = command.Request;
            if (request.WalletTagId is Guid tagId)
            {
                EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, tagId, user.Id);
            }

            var transaction = Transaction.Create(
                wallet.Id,
                request.Description,
                request.Amount,
                request.Type,
                request.TransactionDate,
                request.WalletTagId,
                request.Notes);
            data.AddTransaction(transaction);
            transactionId = transaction.Id;
        }, cancellationToken);
        return transactionId;
    }
}

public sealed class UpdateTransactionCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<UpdateTransactionCommand>
{
    public Task Handle(UpdateTransactionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data, currentUser);
            var wallet = EnsureCurrentWalletCommandHandler.RequireCurrentWallet(data, currentUser);
            var transaction = EnsureCurrentWalletCommandHandler.RequireOwnedTransaction(data, command.Id, wallet);
            var request = command.Request;
            if (request.WalletTagId is Guid tagId)
            {
                EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, tagId, user.Id);
            }

            transaction.Update(
                request.Description,
                request.Amount,
                request.Type,
                request.TransactionDate,
                request.WalletTagId,
                request.Notes);
            wallet.Touch();
        }, cancellationToken);
}

public sealed class DeleteTransactionCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<DeleteTransactionCommand>
{
    public Task Handle(DeleteTransactionCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var wallet = EnsureCurrentWalletCommandHandler.RequireCurrentWallet(data, currentUser);
            var transaction = EnsureCurrentWalletCommandHandler.RequireOwnedTransaction(data, command.Id, wallet);
            data.Transactions.Remove(transaction);
            wallet.Touch();
        }, cancellationToken);
}

public sealed class CreateWalletTagCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : IRequestHandler<CreateWalletTagCommand, Guid>
{
    public async Task<Guid> Handle(CreateWalletTagCommand command, CancellationToken cancellationToken)
    {
        var tagId = Guid.Empty;
        await repository.UpdateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data, currentUser);
            var request = command.Request;
            var tag = WalletTag.Create(user.Id, request.Name, request.Color);
            data.AddWalletTag(tag);
            tagId = tag.Id;
        }, cancellationToken);
        return tagId;
    }
}

public sealed class UpdateWalletTagCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<UpdateWalletTagCommand>
{
    public Task Handle(UpdateWalletTagCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data, currentUser);
            var tag = EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, command.Id, user.Id);
            var normalizedName = string.Join(' ', command.Request.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
            if (data.WalletTags.Any(candidate => candidate.Id != tag.Id && candidate.UserId == user.Id &&
                string.Equals(candidate.Name, normalizedName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDomainStateException($"Wallet tag '{normalizedName}' already exists for this User.");
            }
            tag.Update(command.Request.Name, command.Request.Color);
        }, cancellationToken);
}

public sealed class DeleteWalletTagCommandHandler(ILevelUpRepository repository, ICurrentUserContext? currentUser = null)
    : RequestHandlerBase(repository), IRequestHandler<DeleteWalletTagCommand>
{
    public Task Handle(DeleteWalletTagCommand command, CancellationToken cancellationToken) =>
        MutateAsync(data =>
        {
            var user = EnsureCurrentWalletCommandHandler.RequireCurrentUser(data, currentUser);
            EnsureCurrentWalletCommandHandler.RequireOwnedTag(data, command.Id, user.Id);
            data.RemoveWalletTag(command.Id);
            data.Wallets.FirstOrDefault(wallet => wallet.UserId == user.Id)?.Touch();
        }, cancellationToken);
}
