using LevelUp.Domain.Books;

namespace LevelUp.Services.Books;

public sealed class BookService
{
    public const int MaximumActiveBooks = 2;

    private readonly List<Book> books = [];
    private int nextId = 1;

    public BookService(IEnumerable<Book>? books = null)
    {
        if (books is null)
        {
            return;
        }

        this.books.AddRange(books);
        nextId = this.books.Count == 0
            ? 1
            : this.books.Max(book => book.Id) + 1;
    }

    public Book CreateBook(
        string title,
        string author,
        int totalPages
    )
    {
        Book book = new()
        {
            Id = nextId++
        };

        book.Configure(title, author, totalPages);

        if (GetReadingBooks().Count < MaximumActiveBooks)
        {
            book.Start();
        }

        books.Add(book);
        return book;
    }

    public IReadOnlyList<Book> GetAll()
    {
        return books.ToList().AsReadOnly();
    }

    public IReadOnlyList<Book> GetReadingBooks()
    {
        return books
            .Where(book => book.Status == BookStatus.Reading)
            .ToList()
            .AsReadOnly();
    }

    public Book? GetById(int id)
    {
        return books.FirstOrDefault(book => book.Id == id);
    }

    public void StartBook(Book book)
    {
        EnsureManaged(book);

        if (GetReadingBooks().Count >= MaximumActiveBooks)
        {
            throw new InvalidOperationException(
                "Apenas dois livros podem ficar em andamento ao mesmo tempo."
            );
        }

        book.Start();
    }

    public void UpdateBook(
        Book book,
        string title,
        string author,
        int totalPages
    )
    {
        EnsureManaged(book);
        book.UpdateDetails(title, author, totalPages);
    }

    public int RecordProgress(Book book, int currentPage)
    {
        EnsureManaged(book);
        return book.RecordProgress(currentPage, DateTime.Now);
    }

    public void ArchiveBook(Book book)
    {
        EnsureManaged(book);
        book.Archive();
    }

    public bool DeleteBook(int id)
    {
        Book? book = GetById(id);
        return book is not null && books.Remove(book);
    }

    private void EnsureManaged(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        if (!books.Any(existing => existing.Id == book.Id))
        {
            throw new InvalidOperationException(
                "O livro não é gerenciado por este serviço."
            );
        }
    }
}
