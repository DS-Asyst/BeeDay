using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeDay.Infrastructure.Persistence.SqlServer.Migrations;

/// <inheritdoc />
public partial class AddTransactionAmountUpperBoundCheckConstraint : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Transactions_Amount",
            table: "Transactions");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Transactions_Amount",
            table: "Transactions",
            sql: "[Amount] > 0 AND [Amount] <= 999999999999");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Transactions_Amount",
            table: "Transactions");

        migrationBuilder.AddCheckConstraint(
            name: "CK_Transactions_Amount",
            table: "Transactions",
            sql: "[Amount] > 0");
    }
}
