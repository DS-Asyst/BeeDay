using LevelUp.Domain;
using LevelUp.Infrastructure.Persistence.Entities;
using LevelUp.Domain.Habits;
using LevelUp.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Infrastructure.Persistence;

public sealed class SqliteGameDataStore : IGameDataStore, IDisposable
{
    private readonly DbContextOptions<LevelUpDbContext> options;
    private readonly GameDataValidator validator = new();

    public SqliteGameDataStore(string? databasePath = null)
    {
        DatabasePath = Path.GetFullPath(databasePath ?? LevelUpPaths.GetDefaultDatabasePath());
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        options = new DbContextOptionsBuilder<LevelUpDbContext>()
            .UseSqlite($"Data Source={DatabasePath}")
            .Options;

        using LevelUpDbContext dbContext = CreateContext();
        dbContext.Database.Migrate();
    }

    public string DatabasePath { get; }

    public GameData? Load()
    {
        using LevelUpDbContext dbContext = CreateContext();
        GameMetadataEntity? metadata = dbContext.Metadata.AsNoTracking().SingleOrDefault();
        CharacterEntity? characterEntity = dbContext.Characters.AsNoTracking().SingleOrDefault();
        if (metadata is null || characterEntity is null) return null;

        List<string> titles = dbContext.CharacterTitles.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => x.Title)
            .ToList();

        GameData data = new()
        {
            SchemaVersion = metadata.SchemaVersion,
            SaveRevision = metadata.SaveRevision,
            LastSavedAt = metadata.LastSavedAt,
            Character = ToDomain(characterEntity, titles),
            Habits = dbContext.Habits.AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new Habit
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    AttributeType = x.AttributeType,
                    Direction = x.Direction,
                    PositiveCount = x.PositiveCount,
                    NegativeCount = x.NegativeCount,
                    CreatedAt = x.CreatedAt,
                    LastScoredAt = x.LastScoredAt
                })
                .ToList(),
            Projects = dbContext.Projects.AsNoTracking().OrderBy(x => x.Id).ToList(),
            LegacyQuests = dbContext.Quests.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Tasks = dbContext.Tasks.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Todos = dbContext.Todos.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Milestones = dbContext.Milestones.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Bosses = dbContext.Bosses.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Books = dbContext.Books.AsNoTracking().Include(x => x.ProgressHistory).OrderBy(x => x.Id).ToList(),
            WalletTags = dbContext.WalletTags.AsNoTracking().OrderBy(x => x.Id).ToList(),
            WalletTransactions = dbContext.WalletTransactions.AsNoTracking().OrderBy(x => x.Id).ToList(),
            Achievements = dbContext.Achievements.AsNoTracking().OrderBy(x => x.Id).ToList()
        };

        validator.Validate(data);
        return data;
    }

    public void Save(GameData gameData)
    {
        ArgumentNullException.ThrowIfNull(gameData);
        gameData.SchemaVersion = GameData.CurrentSchemaVersion;
        validator.Validate(gameData);

        using LevelUpDbContext dbContext = CreateContext();
        using var transaction = dbContext.Database.BeginTransaction();

        dbContext.CharacterTitles.ExecuteDelete();
        dbContext.Books.ExecuteDelete();
        dbContext.Bosses.ExecuteDelete();
        dbContext.Todos.ExecuteDelete();
        dbContext.Tasks.ExecuteDelete();
        dbContext.Quests.ExecuteDelete();
        dbContext.Milestones.ExecuteDelete();
        dbContext.WalletTransactions.ExecuteDelete();
        dbContext.WalletTags.ExecuteDelete();
        dbContext.Achievements.ExecuteDelete();
        dbContext.Habits.ExecuteDelete();
        dbContext.Projects.ExecuteDelete();
        dbContext.Characters.ExecuteDelete();
        dbContext.Metadata.ExecuteDelete();

        dbContext.Metadata.Add(new GameMetadataEntity
        {
            Id = 1,
            SchemaVersion = gameData.SchemaVersion,
            SaveRevision = gameData.SaveRevision,
            LastSavedAt = gameData.LastSavedAt,
            UpdatedAtUtc = DateTime.UtcNow
        });

        dbContext.Characters.Add(ToEntity(gameData.Character));
        dbContext.CharacterTitles.AddRange(gameData.Character.Titles.Select(title => new CharacterTitleEntity
        {
            CharacterId = 1,
            Title = title
        }));

        dbContext.Projects.AddRange(gameData.Projects);
        dbContext.Milestones.AddRange(gameData.Milestones);
        dbContext.Quests.AddRange(gameData.LegacyQuests);
        dbContext.Tasks.AddRange(gameData.Tasks);
        dbContext.Todos.AddRange(gameData.Todos);
        dbContext.Bosses.AddRange(gameData.Bosses);
        dbContext.Habits.AddRange(gameData.Habits.Select(ToPersistenceHabit));
        dbContext.Books.AddRange(gameData.Books);
        dbContext.WalletTags.AddRange(gameData.WalletTags);
        dbContext.WalletTransactions.AddRange(gameData.WalletTransactions);
        dbContext.Achievements.AddRange(gameData.Achievements);

        dbContext.SaveChanges();
        transaction.Commit();
    }

    public void Dispose() { }

    private LevelUpDbContext CreateContext() => new(options);


    private static Habit ToPersistenceHabit(Habit habit) => new()
    {
        Id = habit.Id,
        Title = habit.Title,
        Description = habit.Description,
        AttributeType = habit.AttributeType,
        Direction = habit.Direction,
        PositiveCount = habit.PositiveCount,
        NegativeCount = habit.NegativeCount,
        CreatedAt = habit.CreatedAt,
        LastScoredAt = habit.LastScoredAt
    };

    private static CharacterEntity ToEntity(CharacterModel character) => new()
    {
        Id = 1,
        Name = character.Name,
        Class = character.Class,
        Level = character.Level,
        Experience = character.Experience,
        StrengthLevel = character.Attributes.Strength.Level,
        StrengthExperience = character.Attributes.Strength.Experience,
        IntelligenceLevel = character.Attributes.Intelligence.Level,
        IntelligenceExperience = character.Attributes.Intelligence.Experience,
        VitalityLevel = character.Attributes.Vitality.Level,
        VitalityExperience = character.Attributes.Vitality.Experience,
        AgilityLevel = character.Attributes.Agility.Level,
        AgilityExperience = character.Attributes.Agility.Experience,
        LuckLevel = character.Attributes.Luck.Level,
        LuckExperience = character.Attributes.Luck.Experience,
        DexterityLevel = character.Attributes.Dexterity.Level,
        DexterityExperience = character.Attributes.Dexterity.Experience
    };

    private static CharacterModel ToDomain(CharacterEntity entity, List<string> titles) => new()
    {
        Name = entity.Name,
        Class = entity.Class,
        Level = entity.Level,
        Experience = entity.Experience,
        Titles = titles,
        Attributes = new()
        {
            Strength = new() { Level = entity.StrengthLevel, Experience = entity.StrengthExperience },
            Intelligence = new() { Level = entity.IntelligenceLevel, Experience = entity.IntelligenceExperience },
            Vitality = new() { Level = entity.VitalityLevel, Experience = entity.VitalityExperience },
            Agility = new() { Level = entity.AgilityLevel, Experience = entity.AgilityExperience },
            Luck = new() { Level = entity.LuckLevel, Experience = entity.LuckExperience },
            Dexterity = new() { Level = entity.DexterityLevel, Experience = entity.DexterityExperience }
        }
    };
}
