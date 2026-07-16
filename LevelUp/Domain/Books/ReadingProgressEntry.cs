namespace LevelUp.Domain.Books;

public sealed class ReadingProgressEntry
{
    public int PreviousPage { get; set; }

    public int CurrentPage { get; set; }

    public int PagesRead => CurrentPage - PreviousPage;

    public DateTime RecordedAt { get; set; } = DateTime.Now;
}
