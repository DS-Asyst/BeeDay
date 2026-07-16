using LevelUp.Domain.Books;
using LevelUp.Services.Books;
using Xunit;

namespace LevelUp.Tests;

public sealed class BookServiceTests
{
    [Fact]
    public void CreateBook_ShouldAllowOnlyTwoReadingBooks()
    {
        BookService service = new();

        Book first = service.CreateBook("Livro 1", "Autor", 100);
        Book second = service.CreateBook("Livro 2", "Autor", 100);
        Book third = service.CreateBook("Livro 3", "Autor", 100);

        Assert.Equal(BookStatus.Reading, first.Status);
        Assert.Equal(BookStatus.Reading, second.Status);
        Assert.Equal(BookStatus.Locked, third.Status);
    }

    [Fact]
    public void StartBook_ShouldRejectThirdReadingBook()
    {
        BookService service = new();
        service.CreateBook("Livro 1", "Autor", 100);
        service.CreateBook("Livro 2", "Autor", 100);
        Book third = service.CreateBook("Livro 3", "Autor", 100);

        Assert.Throws<InvalidOperationException>(
            () => service.StartBook(third)
        );
    }

    [Fact]
    public void RecordProgress_ShouldStorePagesReadAndCompleteBook()
    {
        BookService service = new();
        Book book = service.CreateBook("Livro", "Autor", 30);

        int firstSession = service.RecordProgress(
            book,
            10,
            new DateTime(2026, 7, 1)
        );
        int secondSession = service.RecordProgress(
            book,
            30,
            new DateTime(2026, 7, 2)
        );

        Assert.Equal(9, firstSession);
        Assert.Equal(20, secondSession);
        Assert.Equal(BookStatus.Completed, book.Status);
        Assert.Equal(2, book.ProgressHistory.Count);
        Assert.Equal(29, book.ProgressHistory.Sum(entry => entry.PagesRead));
    }

    [Fact]
    public void RecordProgress_ShouldRejectPageRegression()
    {
        BookService service = new();
        Book book = service.CreateBook("Livro", "Autor", 100);
        service.RecordProgress(book, 30, DateTime.Today);

        Assert.Throws<InvalidOperationException>(
            () => service.RecordProgress(book, 20, DateTime.Today)
        );
    }
}
