using LevelUp.Application.Common.Contracts;
using LevelUp.Application.Common.Security;
using LevelUp.Application.Features.Wallets.Commands;
using LevelUp.Domain.Entities;
using LevelUp.Domain.Exceptions;
using MediatR;

namespace LevelUp.Application.Features.Wallets.Handlers;

public sealed class EnsureCurrentWalletCommandHandler(IWalletRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<EnsureCurrentWalletCommand, Guid>
{
    public async Task<Guid> Handle(EnsureCurrentWalletCommand request, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var wallet = await repository.GetByUserAsync(userId, cancellationToken);
        if (wallet is null)
        {
            wallet = Wallet.Create(userId);
            await repository.AddAsync(wallet, cancellationToken);
        }

        return wallet.Id;
    }
}

public sealed class CreateTransactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    : IRequestHandler<CreateTransactionCommand, Guid>
{
    public async Task<Guid> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var transactionId = Guid.Empty;

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            // Wallet is created on demand here (not just looked up) — the "Wallet + Transaction" boundary
            // named for IUnitOfWork, since a brand-new Wallet and its first Transaction must persist together.
            var wallet = await unitOfWork.Wallets.GetByUserAsync(userId, cancellationToken);
            if (wallet is null)
            {
                wallet = Wallet.Create(userId);
                await unitOfWork.Wallets.AddAsync(wallet, cancellationToken);
            }

            var request = command.Request;
            if (request.WalletTagId is Guid tagId)
            {
                await WalletLookup.RequireOwnedTagAsync(unitOfWork.WalletTags, userId, tagId, cancellationToken);
            }

            var transaction = Transaction.Create(
                wallet.Id,
                request.Description,
                request.Amount,
                request.Type,
                request.TransactionDate,
                request.WalletTagId,
                request.Notes);
            await unitOfWork.Transactions.AddAsync(transaction, cancellationToken);
            await unitOfWork.Wallets.UpdateAsync(userId, w => w.Touch(), cancellationToken);
            transactionId = transaction.Id;

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }

        return transactionId;
    }
}

public sealed class UpdateTransactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    : IRequestHandler<UpdateTransactionCommand>
{
    public async Task Handle(UpdateTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var wallet = await WalletLookup.RequireCurrentWalletAsync(unitOfWork.Wallets, userId, cancellationToken);
            await WalletLookup.RequireOwnedTransactionAsync(unitOfWork.Transactions, wallet.Id, command.Id, cancellationToken);

            var request = command.Request;
            if (request.WalletTagId is Guid tagId)
            {
                await WalletLookup.RequireOwnedTagAsync(unitOfWork.WalletTags, userId, tagId, cancellationToken);
            }

            await unitOfWork.Transactions.UpdateAsync(
                wallet.Id,
                command.Id,
                transaction => transaction.Update(
                    request.Description,
                    request.Amount,
                    request.Type,
                    request.TransactionDate,
                    request.WalletTagId,
                    request.Notes),
                cancellationToken);
            await unitOfWork.Wallets.UpdateAsync(userId, w => w.Touch(), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}

public sealed class DeleteTransactionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    : IRequestHandler<DeleteTransactionCommand>
{
    public async Task Handle(DeleteTransactionCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var wallet = await WalletLookup.RequireCurrentWalletAsync(unitOfWork.Wallets, userId, cancellationToken);
            var transaction = await WalletLookup.RequireOwnedTransactionAsync(unitOfWork.Transactions, wallet.Id, command.Id, cancellationToken);

            await unitOfWork.Transactions.RemoveAsync(transaction, cancellationToken);
            await unitOfWork.Wallets.UpdateAsync(userId, w => w.Touch(), cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}

public sealed class CreateWalletTagCommandHandler(IWalletTagRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<CreateWalletTagCommand, Guid>
{
    public async Task<Guid> Handle(CreateWalletTagCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        var request = command.Request;
        var tag = WalletTag.Create(userId, request.Name, request.Color);

        if (await repository.IsNameInUseAsync(userId, tag.Name, excludingTagId: null, cancellationToken))
        {
            throw new InvalidDomainStateException($"Wallet tag '{tag.Name}' already exists for this User.");
        }

        await repository.AddAsync(tag, cancellationToken);
        return tag.Id;
    }
}

public sealed class UpdateWalletTagCommandHandler(IWalletTagRepository repository, ICurrentUserContext currentUser)
    : IRequestHandler<UpdateWalletTagCommand>
{
    public async Task Handle(UpdateWalletTagCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);
        await WalletLookup.RequireOwnedTagAsync(repository, userId, command.Id, cancellationToken);

        var request = command.Request;
        var normalizedName = string.Join(' ', request.Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (await repository.IsNameInUseAsync(userId, normalizedName, command.Id, cancellationToken))
        {
            throw new InvalidDomainStateException($"Wallet tag '{normalizedName}' already exists for this User.");
        }

        await repository.UpdateAsync(userId, command.Id, tag => tag.Update(request.Name, request.Color), cancellationToken);
    }
}

public sealed class DeleteWalletTagCommandHandler(IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    : IRequestHandler<DeleteWalletTagCommand>
{
    public async Task Handle(DeleteWalletTagCommand command, CancellationToken cancellationToken)
    {
        var userId = CurrentUserGuard.RequireUserId(currentUser);

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var tag = await WalletLookup.RequireOwnedTagAsync(unitOfWork.WalletTags, userId, command.Id, cancellationToken);

            // Mirrors LevelUpData.RemoveWalletTag exactly: clear every Transaction's reference to this
            // tag before the tag itself is removed, then touch the Wallet — the "WalletTag + Transactions"
            // boundary named for IUnitOfWork.
            await unitOfWork.Transactions.ClearTagReferencesAsync(tag.Id, cancellationToken);
            await unitOfWork.WalletTags.RemoveAsync(tag, cancellationToken);

            var wallet = await unitOfWork.Wallets.GetByUserAsync(userId, cancellationToken);
            if (wallet is not null)
            {
                await unitOfWork.Wallets.UpdateAsync(userId, w => w.Touch(), cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        finally
        {
            await unitOfWork.DisposeAsync();
        }
    }
}

/// <summary>Preserves the exact "not found"/"does not belong" messages the JSON-era handlers used to throw.</summary>
internal static class WalletLookup
{
    public static async Task<Wallet> RequireCurrentWalletAsync(
        IWalletRepository repository,
        Guid userId,
        CancellationToken cancellationToken) =>
        await repository.GetByUserAsync(userId, cancellationToken)
            ?? throw new InvalidDomainStateException("Current User does not have a Wallet.");

    public static async Task<WalletTag> RequireOwnedTagAsync(
        IWalletTagRepository repository,
        Guid userId,
        Guid tagId,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(userId, tagId, cancellationToken)
            ?? throw new InvalidDomainStateException("Wallet tag does not belong to the current User.");

    public static async Task<Transaction> RequireOwnedTransactionAsync(
        ITransactionRepository repository,
        Guid walletId,
        Guid transactionId,
        CancellationToken cancellationToken) =>
        await repository.GetAsync(walletId, transactionId, cancellationToken)
            ?? throw new InvalidDomainStateException("Transaction does not belong to the current User.");
}
