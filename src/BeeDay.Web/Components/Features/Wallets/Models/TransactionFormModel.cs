using System.ComponentModel.DataAnnotations;
using LevelUp.Domain.Enums;

namespace LevelUp.Web.Components.Features.Wallets.Models;

public sealed class TransactionFormModel
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Expense;

    [Required]
    public DateTime TransactionDate { get; set; } = DateTime.Today;

    public Guid? WalletTagId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
