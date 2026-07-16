using System.Text.Json.Serialization;

namespace LevelUp.Domain.Books;

public sealed class Book
{
    public int Id { get; set; }

    [JsonInclude]
    public string Title { get; private set; } = string.Empty;

    [JsonInclude]
    public string Author { get; private set; } = string.Empty;

    [JsonInclude]
    public int TotalPages { get; private set; }

    [JsonInclude]
    public int CurrentPage { get; private set; }

    [JsonInclude]
    public BookStatus Status { get; private set; } = BookStatus.Locked;

    public DateTime CreatedAt { get; init; } = DateTime.Now;

    [JsonInclude]
    public DateTime? StartedAt { get; private set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
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
                "O livro já foi configurado."
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
                "O total de páginas não pode ser menor que a página atual."
            );
        }

        ApplyDetails(title, author, totalPages);
    }

    public void Start()
    {
        if (Status != BookStatus.Locked)
        {
            throw new InvalidOperationException(
                "Apenas livros bloqueados podem ser iniciados."
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
                "O progresso só pode ser registrado em livros em andamento."
            );
        }

        if (currentPage < CurrentPage)
        {
            throw new InvalidOperationException(
                "A página atual não pode ser menor que a última página registrada."
            );
        }

        if (currentPage > TotalPages)
        {
            throw new ArgumentOutOfRangeException(
                nameof(currentPage),
                "A página atual não pode ultrapassar o total do livro."
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
                "O livro já está arquivado."
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
                "O livro deve possuir pelo menos uma página."
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
                "Livros arquivados não podem ser modificados."
            );
        }
    }
}
