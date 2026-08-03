using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions", table =>
        {
            table.HasCheckConstraint("CK_Transactions_Type", "[Type] IN (1, 2)");
            table.HasCheckConstraint("CK_Transactions_Amount", "[Amount] > 0");
        });

        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Description)
            .HasMaxLength(Transaction.MaximumDescriptionLength)
            .IsRequired();

        builder.Property(transaction => transaction.Notes)
            .HasMaxLength(Transaction.MaximumNotesLength)
            .IsRequired();

        builder.Property(transaction => transaction.Type)
            .HasConversion<byte>();

        // Computed property (get-only, derived from Amount/Type) — must be explicitly ignored, same
        // reason as elsewhere in this Sprint's configurations.
        builder.Ignore(transaction => transaction.SignedAmount);

        builder.HasIndex(transaction => new { transaction.WalletId, transaction.TransactionDate })
            .HasDatabaseName("IX_Transactions_Wallet_Date");

        builder.HasIndex(transaction => transaction.WalletTagId)
            .HasDatabaseName("IX_Transactions_Tag");

        builder.HasOne<Wallet>()
            .WithMany()
            .HasForeignKey(transaction => transaction.WalletId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_Transactions_Wallets_WalletId");

        // SET NULL, not CASCADE: a Transaction survives the deletion of the WalletTag it references —
        // the tag itself is removed, not the transaction (01-relational-model.md §2 "Transactions",
        // §3 "WalletTag 1 -- * Transactions (SET NULL)").
        builder.HasOne<WalletTag>()
            .WithMany()
            .HasForeignKey(transaction => transaction.WalletTagId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Transactions_WalletTags_WalletTagId");

        // Cross-aggregate invariant (the referenced WalletTag must belong to the same User as the
        // Wallet) is not representable as a simple FK/CHECK — it spans two separate ownership chains
        // and would require a scalar function. Remains an Application-level validation
        // (LevelUpData.ValidateTransactionTagOwnership today), per 01-relational-model.md §2
        // "Transactions" and 06-domain-persistence-map.md §2.7 — not enforced here.
    }
}
