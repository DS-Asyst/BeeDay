using LevelUp.Domain.Books;
using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;

namespace LevelUp.UI.Components.Book;

public sealed class BookCard
{
    private readonly LevelUp.Domain.Books.Book book;

    public BookCard(LevelUp.Domain.Books.Book book)
    {
        this.book = book;
    }

    public Panel Build()
    {
        ProgressBar progress = new ProgressBar()
            .Width(40)
            .MaxValue(100)
            .Value((double)book.ProgressPercentage)
            .HideRemaining()
            .CompletedStyle(new Style(Color.Green));

        return new EntityCard(book.Title, UIIcons.Book)
            .AddText("Autor", book.Author)
            .AddText("Status", DisplayText.For(book.Status))
            .AddText("Página atual", $"{book.CurrentPage} de {book.TotalPages}")
            .AddRenderable("Progresso", progress)
            .AddText("Páginas registradas", book.ProgressHistory.Sum(entry => entry.PagesRead).ToString())
            .Build();
    }
}
