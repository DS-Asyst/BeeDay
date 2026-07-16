using LevelUp.Domain.Books;

namespace LevelUp.Services.Workflows;

public sealed record ReadingProgressResult(
    Book Book,
    int PagesRead,
    decimal ExperienceEarned,
    bool BookCompleted
);
