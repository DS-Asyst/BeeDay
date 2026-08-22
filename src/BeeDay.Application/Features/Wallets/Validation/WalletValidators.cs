using BeeDay.Application.Features.Wallets.Commands;
using BeeDay.Application.Features.Wallets.Queries;
using BeeDay.Application.Features.Wallets.Requests;
using BeeDay.Domain.Entities;
using FluentValidation;

namespace BeeDay.Application.Features.Wallets.Validation;

public sealed class SaveTransactionRequestValidator : AbstractValidator<SaveTransactionRequest>
{
    public SaveTransactionRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(Transaction.MaximumDescriptionLength);
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(Transaction.MaximumAmount)
            .Must(value => decimal.Round(value, 2, MidpointRounding.AwayFromZero) == value)
            .WithMessage("Amount cannot have more than two decimal places.");
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.TransactionDate).NotEqual(default(DateOnly));
        RuleFor(x => x.WalletTagId).Must(id => id is null || id != Guid.Empty)
            .WithMessage("Wallet tag identifier cannot be empty.");
        RuleFor(x => x.Notes).MaximumLength(Transaction.MaximumNotesLength);
    }
}

public sealed class SaveWalletTagRequestValidator : AbstractValidator<SaveWalletTagRequest>
{
    public SaveWalletTagRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(WalletTag.MaximumNameLength);
        RuleFor(x => x.Color).Matches("^#[0-9a-fA-F]{6}$").When(x => !string.IsNullOrWhiteSpace(x.Color));
    }
}

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator() => RuleFor(x => x.Request).SetValidator(new SaveTransactionRequestValidator());
}

public sealed class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new SaveTransactionRequestValidator());
    }
}

public sealed class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class CreateWalletTagCommandValidator : AbstractValidator<CreateWalletTagCommand>
{
    public CreateWalletTagCommandValidator() => RuleFor(x => x.Request).SetValidator(new SaveWalletTagRequestValidator());
}

public sealed class UpdateWalletTagCommandValidator : AbstractValidator<UpdateWalletTagCommand>
{
    public UpdateWalletTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request).SetValidator(new SaveWalletTagRequestValidator());
    }
}

public sealed class DeleteWalletTagCommandValidator : AbstractValidator<DeleteWalletTagCommand>
{
    public DeleteWalletTagCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class GetTransactionByIdQueryValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.Type).Must(type => type is null || Enum.IsDefined(type.Value));
        RuleFor(x => x.SortBy).IsInEnum();
        RuleFor(x => x.SortDirection).IsInEnum();
        RuleFor(x => x.WalletTagId).Must(id => id is null || id != Guid.Empty);
        RuleFor(x => x.MinimumAmount).GreaterThanOrEqualTo(0).When(x => x.MinimumAmount.HasValue);
        RuleFor(x => x.MaximumAmount).GreaterThanOrEqualTo(0).When(x => x.MaximumAmount.HasValue);
        RuleFor(x => x).Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Start date cannot be after end date.");
        RuleFor(x => x).Must(x => !x.MinimumAmount.HasValue || !x.MaximumAmount.HasValue || x.MinimumAmount <= x.MaximumAmount)
            .WithMessage("Minimum amount cannot exceed maximum amount.");
    }
}
