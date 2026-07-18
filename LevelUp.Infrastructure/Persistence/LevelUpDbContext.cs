using LevelUp.Domain.Achievements;
using LevelUp.Domain.Books;
using LevelUp.Domain.Bosses;
using LevelUp.Domain.Habits;
using LevelUp.Domain.Milestones;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;
using LevelUp.Domain.Wallet;
using LevelUp.Domain.Tasks;
using LevelUp.Domain.Todos;
using LevelUp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace LevelUp.Infrastructure.Persistence;

public sealed class LevelUpDbContext(DbContextOptions<LevelUpDbContext> options) : DbContext(options)
{
    public DbSet<GameMetadataEntity> Metadata => Set<GameMetadataEntity>();
    public DbSet<CharacterEntity> Characters => Set<CharacterEntity>();
    public DbSet<CharacterTitleEntity> CharacterTitles => Set<CharacterTitleEntity>();
    public DbSet<Habit> Habits => Set<Habit>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ProjectTodo> Todos => Set<ProjectTodo>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<BossEncounter> Bosses => Set<BossEncounter>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<WalletTag> WalletTags => Set<WalletTag>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<Achievement> Achievements => Set<Achievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameMetadataEntity>(entity =>
        {
            entity.ToTable("GameMetadata", t => t.HasCheckConstraint("CK_GameMetadata_Singleton", "Id = 1"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<CharacterEntity>(entity =>
        {
            entity.ToTable("Characters", t => t.HasCheckConstraint("CK_Characters_Singleton", "Id = 1"));
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Class).HasConversion<string>().HasMaxLength(40);
            ConfigureDecimal(entity.Property(x => x.Experience));
            ConfigureDecimal(entity.Property(x => x.StrengthExperience));
            ConfigureDecimal(entity.Property(x => x.IntelligenceExperience));
            ConfigureDecimal(entity.Property(x => x.VitalityExperience));
            ConfigureDecimal(entity.Property(x => x.AgilityExperience));
            ConfigureDecimal(entity.Property(x => x.LuckExperience));
            ConfigureDecimal(entity.Property(x => x.DexterityExperience));
        });

        modelBuilder.Entity<CharacterTitleEntity>(entity =>
        {
            entity.ToTable("CharacterTitles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(160).IsRequired();
            entity.HasIndex(x => new { x.CharacterId, x.Title }).IsUnique();
            entity.HasOne<CharacterEntity>().WithMany().HasForeignKey(x => x.CharacterId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable("Habits");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.AttributeType).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Direction).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.PositiveCount).IsRequired();
            entity.Property(x => x.NegativeCount).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.LastScoredAt);
            entity.Ignore(x => x.ExperienceReward);
            entity.Ignore(x => x.AttributeExperienceReward);
            entity.Ignore(x => x.AllowsPositive);
            entity.Ignore(x => x.AllowsNegative);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.PrimaryAttribute).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.ToTable("Milestones");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.Ignore(x => x.IsLocked);
            entity.Ignore(x => x.CanAcceptQuests);
            entity.OwnsOne(x => x.Reward, reward =>
            {
                reward.Property(x => x.Experience).HasColumnName("RewardExperience");
                reward.Property(x => x.Gold).HasColumnName("RewardGold");
                reward.Property(x => x.Title).HasColumnName("RewardTitle").HasMaxLength(160);
                reward.Ignore(x => x.HasReward);
            });
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Order }).IsUnique();
        });

        modelBuilder.Entity<Quest>(entity =>
        {
            entity.ToTable("Quests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.AttributeType).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<Milestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.SetNull);
        });


        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.AttributeType).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Recurrence).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.RepeatOn).HasConversion<int>();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<ProjectTodo>(entity =>
        {
            entity.ToTable("Todos"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.AttributeType).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Milestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BossEncounter>(entity =>
        {
            entity.ToTable("Bosses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.AchievementPrefix).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Milestone>().WithMany().HasForeignKey(x => x.MilestoneId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Author).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.Ignore(x => x.ProgressPercentage);
            entity.OwnsMany(x => x.ProgressHistory, progress =>
            {
                progress.ToTable("BookProgressEntries");
                progress.WithOwner().HasForeignKey("BookId");
                progress.Property<int>("Id").ValueGeneratedOnAdd();
                progress.HasKey("Id");
                progress.Ignore(x => x.PagesRead);
                progress.HasIndex("BookId", nameof(ReadingProgressEntry.RecordedAt));
            });
        });

        modelBuilder.Entity<WalletTag>(entity =>
        {
            entity.ToTable("WalletTags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("WalletTransactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);
            ConfigureDecimal(entity.Property(x => x.Amount));
            entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Justification).HasMaxLength(1000);
            entity.Property(x => x.ReversalReason).HasMaxLength(1000);
            entity.Ignore(x => x.IsReversal);
            entity.Ignore(x => x.IsReversed);
            entity.Ignore(x => x.IsCredit);
            entity.Ignore(x => x.IsDebit);
            entity.HasOne<WalletTag>().WithMany().HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<WalletTransaction>().WithMany().HasForeignKey(x => x.ReversalOfTransactionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.ToTable("Achievements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            entity.HasIndex(x => x.Code).IsUnique();
        });
    }

    private static void ConfigureDecimal(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<decimal> property)
        => property.HasPrecision(18, 2);
}
