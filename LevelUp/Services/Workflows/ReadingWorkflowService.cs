using LevelUp.Domain.Books;
using LevelUp.Domain.Rewards;
using LevelUp.Services.Books;
using LevelUp.Services.Character;
using LevelUp.Services.Persistence;
using CharacterModel = LevelUp.Domain.Character.Character;

namespace LevelUp.Services.Workflows;

public sealed class ReadingWorkflowService
{
    public const decimal ExperiencePerPage = 0.5m;

    private readonly BookService bookService;
    private readonly CharacterService characterService;
    private readonly CharacterModel character;
    private readonly GameStateService gameStateService;

    public ReadingWorkflowService(
        BookService bookService,
        CharacterService characterService,
        CharacterModel character,
        GameStateService gameStateService
    )
    {
        this.bookService = bookService;
        this.characterService = characterService;
        this.character = character;
        this.gameStateService = gameStateService;
    }

    public ReadingProgressResult RecordProgress(
        Book book,
        int currentPage,
        DateTime recordedAt
    )
    {
        int pagesRead = bookService.RecordProgress(
            book,
            currentPage,
            recordedAt
        );

        decimal experience = pagesRead * ExperiencePerPage;

        if (experience > 0)
        {
            character.ApplyReward(new Reward(Experience: experience));
        }

        gameStateService.Save();

        return new ReadingProgressResult(
            book,
            pagesRead,
            experience,
            book.Status == BookStatus.Completed
        );
    }
}
