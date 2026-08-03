using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeDay.Infrastructure.Persistence.SqlServer.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Language = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                Theme = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                LastLoginAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                HasCompletedOnboarding = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                EmailConfirmedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                Nickname = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Avatar = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                SessionVersion = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
                table.CheckConstraint("CK_Users_Language", "[Language] IN (0, 1)");
                table.CheckConstraint("CK_Users_Theme", "[Theme] IN (0, 1, 2)");
            });

        migrationBuilder.CreateTable(
            name: "ExperienceEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<long>(type: "bigint", nullable: false),
                RewardType = table.Column<byte>(type: "tinyint", nullable: false),
                ExperienceBefore = table.Column<long>(type: "bigint", nullable: false),
                ExperienceAfter = table.Column<long>(type: "bigint", nullable: false),
                LevelBefore = table.Column<int>(type: "int", nullable: false),
                LevelAfter = table.Column<int>(type: "int", nullable: false),
                GrantedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                SourceDescription = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                SourceType = table.Column<byte>(type: "tinyint", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExperienceEntries", x => x.Id);
                table.CheckConstraint("CK_ExperienceEntries_RewardType", "[RewardType] = 0");
                table.CheckConstraint("CK_ExperienceEntries_SourceType", "[SourceType] BETWEEN 0 AND 6");
                table.ForeignKey(
                    name: "FK_ExperienceEntries_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Habits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Attribute = table.Column<byte>(type: "tinyint", nullable: true),
                Completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                Direction = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)2),
                Difficulty = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                ResetCounter = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                PositiveCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                NegativeCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                Position = table.Column<int>(type: "int", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Habits", x => x.Id);
                table.CheckConstraint("CK_Habits_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
                table.CheckConstraint("CK_Habits_Difficulty", "[Difficulty] IN (0, 1, 2, 3)");
                table.CheckConstraint("CK_Habits_Direction", "[Direction] IN (0, 1, 2)");
                table.CheckConstraint("CK_Habits_ResetCounter", "[ResetCounter] IN (0, 1, 2)");
                table.ForeignKey(
                    name: "FK_Habits_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Attribute = table.Column<byte>(type: "tinyint", nullable: true),
                Completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                Color = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false, defaultValue: "#7A4FCB"),
                ExpectedDate = table.Column<DateOnly>(type: "date", nullable: true),
                Archived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Position = table.Column<int>(type: "int", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Projects", x => x.Id);
                table.CheckConstraint("CK_Projects_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
                table.CheckConstraint("CK_Projects_Color", "[Color] LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'");
                table.ForeignKey(
                    name: "FK_Projects_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RecurringTasks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Attribute = table.Column<byte>(type: "tinyint", nullable: true),
                Completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                Repeat = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                Position = table.Column<int>(type: "int", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecurringTasks", x => x.Id);
                table.CheckConstraint("CK_RecurringTasks_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
                table.CheckConstraint("CK_RecurringTasks_Repeat", "[Repeat] IN (0, 1, 2, 3)");
                table.ForeignKey(
                    name: "FK_RecurringTasks_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserExperience",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TotalExperience = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserExperience", x => x.UserId);
                table.ForeignKey(
                    name: "FK_UserExperience_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "UserTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Type = table.Column<byte>(type: "tinyint", nullable: false),
                TokenHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UsedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                RevokedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserTokens", x => x.Id);
                table.CheckConstraint("CK_UserTokens_Type", "[Type] IN (1, 2)");
                table.ForeignKey(
                    name: "FK_UserTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Wallets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Wallets", x => x.Id);
                table.ForeignKey(
                    name: "FK_Wallets_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "WalletTags",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                Color = table.Column<string>(type: "nchar(7)", fixedLength: true, maxLength: 7, nullable: false, defaultValue: "#7A4FCB"),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WalletTags", x => x.Id);
                table.CheckConstraint("CK_WalletTags_Color", "[Color] LIKE '#[0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F][0-9A-F]'");
                table.ForeignKey(
                    name: "FK_WalletTags_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Todos",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Featured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                Attribute = table.Column<byte>(type: "tinyint", nullable: true),
                Completed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DueDate = table.Column<DateOnly>(type: "date", nullable: true),
                Position = table.Column<int>(type: "int", nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Todos", x => x.Id);
                table.CheckConstraint("CK_Todos_Attribute", "[Attribute] IN (1, 2, 3, 4) OR [Attribute] IS NULL");
                table.ForeignKey(
                    name: "FK_Todos_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Todos_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "Transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                WalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Description = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(19,2)", precision: 19, scale: 2, nullable: false),
                Type = table.Column<byte>(type: "tinyint", nullable: false),
                TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                WalletTagId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset(7)", precision: 7, nullable: false),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transactions", x => x.Id);
                table.CheckConstraint("CK_Transactions_Amount", "[Amount] > 0");
                table.CheckConstraint("CK_Transactions_Type", "[Type] IN (1, 2)");
                table.ForeignKey(
                    name: "FK_Transactions_WalletTags_WalletTagId",
                    column: x => x.WalletTagId,
                    principalTable: "WalletTags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Transactions_Wallets_WalletId",
                    column: x => x.WalletId,
                    principalTable: "Wallets",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ExperienceEntries_User_Time",
            table: "ExperienceEntries",
            columns: new[] { "UserId", "GrantedAtUtc" });

        // Dedup rule (01-relational-model.md §2 "ExperienceEntries"; ExperienceEntryConfiguration.cs
        // has the full rationale): added via raw SQL, not migrationBuilder.CreateIndex, because this
        // index spans ExperienceEntries' own UserId/RewardType columns and SourceType/SourceId,
        // which live on the Source complex type — confirmed during this Sprint that no EF Core
        // Fluent API or metadata surface can express an index crossing that boundary.
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX [UX_ExperienceEntries_Dedup] ON [ExperienceEntries] ([UserId], [SourceType], [SourceId], [RewardType]) WHERE [SourceId] IS NOT NULL AND [SourceType] <> 0;");

        migrationBuilder.CreateIndex(
            name: "IX_Habits_User_Position",
            table: "Habits",
            columns: new[] { "UserId", "Position" });

        migrationBuilder.CreateIndex(
            name: "IX_Projects_User_Position",
            table: "Projects",
            columns: new[] { "UserId", "Position" });

        migrationBuilder.CreateIndex(
            name: "IX_RecurringTasks_User_Position",
            table: "RecurringTasks",
            columns: new[] { "UserId", "Position" });

        migrationBuilder.CreateIndex(
            name: "IX_Todos_Project_Position",
            table: "Todos",
            columns: new[] { "ProjectId", "Position" });

        migrationBuilder.CreateIndex(
            name: "IX_Todos_User",
            table: "Todos",
            columns: new[] { "UserId", "Id" });

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_Tag",
            table: "Transactions",
            column: "WalletTagId");

        migrationBuilder.CreateIndex(
            name: "IX_Transactions_Wallet_Date",
            table: "Transactions",
            columns: new[] { "WalletId", "TransactionDate" });

        migrationBuilder.CreateIndex(
            name: "UX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_Users_Nickname",
            table: "Users",
            column: "Nickname",
            unique: true,
            filter: "[Nickname] <> N''");

        migrationBuilder.CreateIndex(
            name: "IX_UserTokens_User",
            table: "UserTokens",
            columns: new[] { "UserId", "Type", "ExpiresAtUtc" });

        migrationBuilder.CreateIndex(
            name: "UX_UserTokens_Hash",
            table: "UserTokens",
            columns: new[] { "TokenHash", "Type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_Wallets_User",
            table: "Wallets",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "UX_WalletTags_User_Name",
            table: "WalletTags",
            columns: new[] { "UserId", "Name" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX [UX_ExperienceEntries_Dedup] ON [ExperienceEntries];");

        migrationBuilder.DropTable(
            name: "ExperienceEntries");

        migrationBuilder.DropTable(
            name: "Habits");

        migrationBuilder.DropTable(
            name: "RecurringTasks");

        migrationBuilder.DropTable(
            name: "Todos");

        migrationBuilder.DropTable(
            name: "Transactions");

        migrationBuilder.DropTable(
            name: "UserExperience");

        migrationBuilder.DropTable(
            name: "UserTokens");

        migrationBuilder.DropTable(
            name: "Projects");

        migrationBuilder.DropTable(
            name: "WalletTags");

        migrationBuilder.DropTable(
            name: "Wallets");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
