using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LevelUp.Infrastructure.Persistence.Migrations;

[DbContext(typeof(LevelUpDbContext))]
[Migration("20260718000000_InitialSchema7")]
public partial class InitialSchema7 : Migration
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

        migrationBuilder.CreateTable(
            name: "Characters",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                Class = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                Level = table.Column<int>(type: "INTEGER", nullable: false),
                Experience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                StrengthLevel = table.Column<int>(type: "INTEGER", nullable: false),
                StrengthExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                IntelligenceLevel = table.Column<int>(type: "INTEGER", nullable: false),
                IntelligenceExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                VitalityLevel = table.Column<int>(type: "INTEGER", nullable: false),
                VitalityExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                AgilityLevel = table.Column<int>(type: "INTEGER", nullable: false),
                AgilityExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                LuckLevel = table.Column<int>(type: "INTEGER", nullable: false),
                LuckExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                DexterityLevel = table.Column<int>(type: "INTEGER", nullable: false),
                DexterityExperience = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Characters", x => x.Id);
                table.CheckConstraint("CK_Characters_Singleton", "Id = 1");
            });

        migrationBuilder.CreateTable("Habits", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 2000, nullable: false),
            AttributeType = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Direction = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            PositiveCount = table.Column<int>("INTEGER", nullable: false),
            NegativeCount = table.Column<int>("INTEGER", nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            LastScoredAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Habits", x => x.Id));

        migrationBuilder.CreateTable("Projects", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            PrimaryAttribute = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            CompletedAt = table.Column<DateTime>("TEXT", nullable: true),
            ArchivedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Projects", x => x.Id));

        migrationBuilder.CreateTable("Books", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Title = table.Column<string>("TEXT", maxLength: 300, nullable: false),
            Author = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            TotalPages = table.Column<int>("INTEGER", nullable: false),
            CurrentPage = table.Column<int>("INTEGER", nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            StartedAt = table.Column<DateTime>("TEXT", nullable: true),
            CompletedAt = table.Column<DateTime>("TEXT", nullable: true),
            ArchivedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Books", x => x.Id));

        migrationBuilder.CreateTable("WalletTags", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Name = table.Column<string>("TEXT", maxLength: 120, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            UpdatedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_WalletTags", x => x.Id));

        migrationBuilder.CreateTable("Achievements", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Code = table.Column<string>("TEXT", maxLength: 120, nullable: false),
            Name = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 2000, nullable: false),
            Category = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            SourceId = table.Column<int>("INTEGER", nullable: true),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            UnlockedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table => table.PrimaryKey("PK_Achievements", x => x.Id));

        migrationBuilder.CreateTable("CharacterTitles", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            CharacterId = table.Column<int>("INTEGER", nullable: false),
            Title = table.Column<string>("TEXT", maxLength: 160, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_CharacterTitles", x => x.Id);
            table.ForeignKey("FK_CharacterTitles_Characters_CharacterId", x => x.CharacterId, "Characters", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("BookProgressEntries", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            BookId = table.Column<int>("INTEGER", nullable: false),
            PreviousPage = table.Column<int>("INTEGER", nullable: false),
            CurrentPage = table.Column<int>("INTEGER", nullable: false),
            RecordedAt = table.Column<DateTime>("TEXT", nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_BookProgressEntries", x => x.Id);
            table.ForeignKey("FK_BookProgressEntries_Books_BookId", x => x.BookId, "Books", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("Milestones", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ProjectId = table.Column<int>("INTEGER", nullable: false),
            Order = table.Column<int>("INTEGER", nullable: false),
            RequiredCompletedQuests = table.Column<int>("INTEGER", nullable: false),
            Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            RewardExperience = table.Column<int>("INTEGER", nullable: false),
            RewardGold = table.Column<int>("INTEGER", nullable: false),
            RewardTitle = table.Column<string>("TEXT", maxLength: 160, nullable: true),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            UnlockedAt = table.Column<DateTime>("TEXT", nullable: true),
            ActivatedAt = table.Column<DateTime>("TEXT", nullable: true),
            CompletedAt = table.Column<DateTime>("TEXT", nullable: true),
            ArchivedAt = table.Column<DateTime>("TEXT", nullable: true),
            RewardClaimedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Milestones", x => x.Id);
            table.ForeignKey("FK_Milestones_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
        });

        migrationBuilder.CreateTable("WalletTransactions", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Type = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Amount = table.Column<decimal>("TEXT", precision: 18, scale: 2, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 500, nullable: false),
            TagId = table.Column<int>("INTEGER", nullable: true),
            Justification = table.Column<string>("TEXT", maxLength: 1000, nullable: false),
            OccurredAt = table.Column<DateTime>("TEXT", nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            UpdatedAt = table.Column<DateTime>("TEXT", nullable: true),
            ReversalOfTransactionId = table.Column<int>("INTEGER", nullable: true),
            ReversedAt = table.Column<DateTime>("TEXT", nullable: true),
            ReversalReason = table.Column<string>("TEXT", maxLength: 1000, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_WalletTransactions", x => x.Id);
            table.ForeignKey("FK_WalletTransactions_WalletTags_TagId", x => x.TagId, "WalletTags", "Id", onDelete: ReferentialAction.SetNull);
            table.ForeignKey("FK_WalletTransactions_WalletTransactions_ReversalOfTransactionId", x => x.ReversalOfTransactionId, "WalletTransactions", "Id", onDelete: ReferentialAction.Restrict);
        });

        migrationBuilder.CreateTable("Quests", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ProjectId = table.Column<int>("INTEGER", nullable: true),
            MilestoneId = table.Column<int>("INTEGER", nullable: true),
            Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            AttributeType = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            ActivatedAt = table.Column<DateTime>("TEXT", nullable: true),
            CompletedAt = table.Column<DateTime>("TEXT", nullable: true),
            ArchivedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Quests", x => x.Id);
            table.ForeignKey("FK_Quests_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.SetNull);
            table.ForeignKey("FK_Quests_Milestones_MilestoneId", x => x.MilestoneId, "Milestones", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateTable("Tasks", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            AttributeType = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Recurrence = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            RepeatOn = table.Column<int>("INTEGER", nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            LastCompletedAt = table.Column<DateTime>("TEXT", nullable: true),
            CompletionCount = table.Column<int>("INTEGER", nullable: false)
        }, constraints: table => table.PrimaryKey("PK_Tasks", x => x.Id));

        migrationBuilder.CreateTable("Todos", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ProjectId = table.Column<int>("INTEGER", nullable: false),
            MilestoneId = table.Column<int>("INTEGER", nullable: true),
            Title = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            AttributeType = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            ActivatedAt = table.Column<DateTime>("TEXT", nullable: true),
            CompletedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Todos", x => x.Id);
            table.ForeignKey("FK_Todos_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Todos_Milestones_MilestoneId", x => x.MilestoneId, "Milestones", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateTable("Bosses", table => new
        {
            Id = table.Column<int>("INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
            ProjectId = table.Column<int>("INTEGER", nullable: false),
            MilestoneId = table.Column<int>("INTEGER", nullable: true),
            Name = table.Column<string>("TEXT", maxLength: 200, nullable: false),
            Description = table.Column<string>("TEXT", maxLength: 4000, nullable: false),
            AchievementPrefix = table.Column<string>("TEXT", maxLength: 160, nullable: false),
            IsFinalBoss = table.Column<bool>("INTEGER", nullable: false),
            Status = table.Column<string>("TEXT", maxLength: 40, nullable: false),
            CreatedAt = table.Column<DateTime>("TEXT", nullable: false),
            UnlockedAt = table.Column<DateTime>("TEXT", nullable: true),
            DefeatedAt = table.Column<DateTime>("TEXT", nullable: true),
            ArchivedAt = table.Column<DateTime>("TEXT", nullable: true)
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Bosses", x => x.Id);
            table.ForeignKey("FK_Bosses_Projects_ProjectId", x => x.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Bosses_Milestones_MilestoneId", x => x.MilestoneId, "Milestones", "Id", onDelete: ReferentialAction.SetNull);
        });

        migrationBuilder.CreateIndex("IX_Todos_MilestoneId", "Todos", "MilestoneId");
        migrationBuilder.CreateIndex("IX_Todos_ProjectId", "Todos", "ProjectId");
        migrationBuilder.CreateIndex("IX_Achievements_Code", "Achievements", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_BookProgressEntries_BookId_RecordedAt", "BookProgressEntries", new[] { "BookId", "RecordedAt" });
        migrationBuilder.CreateIndex("IX_Bosses_MilestoneId", "Bosses", "MilestoneId");
        migrationBuilder.CreateIndex("IX_Bosses_ProjectId", "Bosses", "ProjectId");
        migrationBuilder.CreateIndex("IX_CharacterTitles_CharacterId_Title", "CharacterTitles", new[] { "CharacterId", "Title" }, unique: true);
        migrationBuilder.CreateIndex("IX_Milestones_ProjectId_Order", "Milestones", new[] { "ProjectId", "Order" }, unique: true);
        migrationBuilder.CreateIndex("IX_Quests_MilestoneId", "Quests", "MilestoneId");
        migrationBuilder.CreateIndex("IX_Quests_ProjectId", "Quests", "ProjectId");
        migrationBuilder.CreateIndex("IX_WalletTags_Name", "WalletTags", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_WalletTransactions_ReversalOfTransactionId", "WalletTransactions", "ReversalOfTransactionId");
        migrationBuilder.CreateIndex("IX_WalletTransactions_TagId", "WalletTransactions", "TagId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Achievements");
        migrationBuilder.DropTable("BookProgressEntries");
        migrationBuilder.DropTable("Bosses");
        migrationBuilder.DropTable("CharacterTitles");
        migrationBuilder.DropTable("Habits");
        migrationBuilder.DropTable("Quests");
        migrationBuilder.DropTable("Tasks");
        migrationBuilder.DropTable("Todos");
        migrationBuilder.DropTable("WalletTransactions");
        migrationBuilder.DropTable("Books");
        migrationBuilder.DropTable("Characters");
        migrationBuilder.DropTable("Milestones");
        migrationBuilder.DropTable("WalletTags");
        migrationBuilder.DropTable("Projects");
        migrationBuilder.DropTable("GameMetadata");
    }
}
