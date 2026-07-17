using LevelUp.Domain.Books;
using LevelUp.Domain.Rewards;
using LevelUp.Services.Achievements;
using LevelUp.Services.Books;
using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Workflows;

public sealed class ReadingWorkflowService
{
    private readonly BookService bookService;
    private readonly AchievementService achievementService;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public ReadingWorkflowService(BookService bookService, AchievementService achievementService, CharacterModel character, GameStateService gameStateService)
    {
        this.bookService = bookService;
        this.achievementService = achievementService;
        this.character = character;
        this.gameStateService = gameStateService;
    }

    public ReadingProgressResult RecordProgress(Book book, int currentPage)
    {
        bool wasCompleted = book.Status == BookStatus.Completed;
        int pagesRead = bookService.RecordProgress(book, currentPage);
        bool completedNow = !wasCompleted && book.Status == BookStatus.Completed;
        Reward reward = completedNow ? CreateCompletionReward(book.TotalPages) : Reward.None;
        character.ApplyReward(reward);
        if (completedNow)
        {
            int completedBooks = bookService.GetAll().Count(item => item.Status == BookStatus.Completed);
            achievementService.UnlockReadingAchievements(completedBooks);
        }
        gameStateService.Save();
        return new ReadingProgressResult(book, pagesRead, reward, completedNow);
    }

    public static Reward CreateCompletionReward(int totalPages)
    {
        if (totalPages <= 0) throw new ArgumentOutOfRangeException(nameof(totalPages));
        decimal experience = totalPages < 100 ? 1m : Math.Floor(totalPages * 0.10m);
        return new Reward(Experience: experience);
    }
}
