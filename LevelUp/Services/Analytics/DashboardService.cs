using LevelUp.Application;
using LevelUp.Domain.Books;
using LevelUp.Domain.Projects;
using LevelUp.Domain.Quests;

namespace LevelUp.Services.Analytics;

public sealed class DashboardService
{
    private readonly GameSession session;

    public DashboardService(GameSession session)
    {
        this.session = session;
    }

    public DashboardSnapshot GetSnapshot(DateTime referenceDate)
    {
        int pagesRead = session.Books.GetAll()
            .SelectMany(book => book.ProgressHistory)
            .Where(entry =>
                entry.RecordedAt.Year == referenceDate.Year &&
                entry.RecordedAt.Month == referenceDate.Month
            )
            .Sum(entry => entry.PagesRead);

        return new DashboardSnapshot(
            session.Character.Level,
            session.Character.Rank,
            session.Character.Experience,
            session.Character.ExperienceToNextLevel,
            session.Projects.GetAllProjects().Count(project => project.Status == ProjectStatus.Active),
            session.Quests.GetAllQuests().Count(quest => quest.Status == QuestStatus.Active),
            session.Quests.GetAllQuests().Count(quest => quest.Status == QuestStatus.Completed),
            session.Books.GetAll().Count(book => book.Status == BookStatus.Reading),
            pagesRead,
            session.Wallet.Balance,
            session.Wallet.GetMonthlyBalance(referenceDate.Year, referenceDate.Month),
            session.Achievements.GetUnlocked().Count
        );
    }
}
