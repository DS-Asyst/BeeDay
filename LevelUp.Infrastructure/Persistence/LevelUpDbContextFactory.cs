using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LevelUp.Infrastructure.Persistence;

public sealed class LevelUpDbContextFactory : IDesignTimeDbContextFactory<LevelUpDbContext>
{
    public LevelUpDbContext CreateDbContext(string[] args)
    {
        string databasePath = args.FirstOrDefault() ?? LevelUpPaths.GetDefaultDatabasePath();
        DbContextOptionsBuilder<LevelUpDbContext> options = new();
        options.UseSqlite($"Data Source={databasePath}");
        return new LevelUpDbContext(options.Options);
    }
}
