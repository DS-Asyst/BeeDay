using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelUp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LevelUpDbContext))]
[Migration("20260717000000_InitialSqlitePersistence")]
public partial class InitialSqlitePersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameMetadata",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false),
                SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                SaveRevision = table.Column<int>(type: "INTEGER", nullable: false),
                LastSavedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameMetadata", x => x.Id);
                table.CheckConstraint("CK_GameMetadata_Singleton", "Id = 1");
            });

        foreach (string tableName in Tables)
        {
            migrationBuilder.CreateTable(
                name: tableName,
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table => table.PrimaryKey($"PK_{tableName}", x => x.Id));
            migrationBuilder.CreateIndex(
                name: $"IX_{tableName}_UpdatedAtUtc",
                table: tableName,
                column: "UpdatedAtUtc");
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (string tableName in Tables.Reverse()) migrationBuilder.DropTable(name: tableName);
        migrationBuilder.DropTable(name: "GameMetadata");
    }

    private static readonly string[] Tables =
    [
        "Characters", "Habits", "Projects", "Quests", "Milestones", "Bosses", "Books",
        "WalletTags", "WalletTransactions", "Achievements"
    ];
}
