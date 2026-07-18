
namespace LevelUp.Domain.Books;

public sealed class Book
{
    public int Id { get; set; }

    public string Title { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    public int TotalPages { get; private set; }

    public int CurrentPage { get; private set; }

    public BookStatus Status { get; private set; } = BookStatus.Locked;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? ArchivedAt { get; private set; }

    public List<ReadingProgressEntry> ProgressHistory { get; set; } = [];

    public decimal ProgressPercentage => TotalPages == 0
        ? 0m
        : Math.Round(CurrentPage * 100m / TotalPages, 2);

    public void Configure(
        string title,
        string author,
        int totalPages
    )
    {
        if (!string.IsNullOrWhiteSpace(Title))
        {
            throw new InvalidOperationException(
                "The book has already been configured."
            );
        }

        ApplyDetails(title, author, totalPages);
    }

    public void UpdateDetails(
        string title,
        string author,
        int totalPages
    )
    {
        EnsureNotArchived();

        if (totalPages < CurrentPage)
        {
            throw new InvalidOperationException(
                "Total pages cannot be less than the current page."
            );
        }

        ApplyDetails(title, author, totalPages);
    }

    public void Start()
    {
        if (Status != BookStatus.Locked)
        {
            throw new InvalidOperationException(
                "Only locked books can be started."
            );
        }

        Status = BookStatus.Reading;
        StartedAt ??= DateTime.Now;
    }

    public int RecordProgress(int currentPage, DateTime recordedAt)
    {
        if (Status != BookStatus.Reading)
        {
            throw new InvalidOperationException(
                "Progress can only be recorded for books in progress."
            );
        }

        if (currentPage < CurrentPage)
        {
            throw new InvalidOperationException(
                "The current page cannot be less than the last recorded page."
            );
        }

        if (currentPage > TotalPages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentPage),
                "The current page cannot exceed the total number of pages."
            );
        }

        int previousPage = CurrentPage;
        CurrentPage = currentPage;

        if (currentPage > previousPage)
        {
            ProgressHistory.Add(new ReadingProgressEntry
            {
                PreviousPage = previousPage,
                CurrentPage = currentPage,
                RecordedAt = recordedAt
            });
        }

        if (CurrentPage == TotalPages)
        {
            Status = BookStatus.Completed;
            CompletedAt = recordedAt;
        }

        return CurrentPage - previousPage;
    }

    public void Archive()
    {
        if (Status == BookStatus.Archived)
        {
            throw new InvalidOperationException(
                "The book is already archived."
            );
        }

        Status = BookStatus.Archived;
        ArchivedAt = DateTime.Now;
    }

    private void ApplyDetails(
        string title,
        string author,
        int totalPages
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);

        if (totalPages <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalPages),
                "The book must have at least one page."
            );
        }

        Title = title.Trim();
        Author = author.Trim();
        TotalPages = totalPages;

    }

    private void EnsureNotArchived()
    {
        if (Status == BookStatus.Archived)
        {
            throw new InvalidOperationException(
                "Archived books cannot be changed."
            );
        }
    }
}
