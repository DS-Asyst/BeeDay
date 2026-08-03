using LevelUp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LevelUp.Infrastructure.Persistence.SqlServer.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(wallet => wallet.Id);

        builder.HasIndex(wallet => wallet.UserId)
            .IsUnique()
            .HasDatabaseName("UX_Wallets_User");

        // NO ACTION, not CASCADE: no Wallet deletion operation exists in Domain/Application today
        // (Aggregate Map §7). When one is added, it will be an explicit Wallet delete, which already
        // cascades to its Transactions (WalletId, CASCADE, see TransactionConfiguration) — this NO
        // ACTION avoids a second cascade path converging on Transactions alongside
        // Users -> WalletTags (CASCADE) -> Transactions (SET NULL), which SQL Server would otherwise
        // reject at schema creation (error 1785). 01-relational-model.md §5.1.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(wallet => wallet.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Wallets_Users_UserId");
    }
}
