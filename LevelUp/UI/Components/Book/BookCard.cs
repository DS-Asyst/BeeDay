using LevelUp.UI.Components.Shared;
using LevelUp.UI.Infrastructure;
using LevelUp.UI.Infrastructure.Themes;
using Spectre.Console;
using BookModel = LevelUp.Domain.Books.Book;

namespace LevelUp.UI.Components.Book;

public sealed class BookCard
{
    private const int ProgressBarWidth = 30;

    private readonly BookModel book;

    public BookCard(BookModel book)
    {
        ArgumentNullException.ThrowIfNull(book);

        this.book = book;
    }

    public Panel Build()
    {
        return new EntityCard(
            book.Title,
            UIIcons.Book
        )
            .AddText(
                "Autor",
                book.Author
            )
            .AddText(
                "Status",
                DisplayText.For(book.Status)
            )
            .AddText(
                "Página atual",
                $"{book.CurrentPage} de {book.TotalPages}"
            )
            .AddRenderable(
                "Progresso",
                BuildProgressBar()
            )
            .AddText(
                "Páginas registradas",
                book.ProgressHistory
                    .Sum(entry => entry.PagesRead)
                    .ToString()
            )
            .Build();
    }

    private Markup BuildProgressBar()
    {
        decimal percentage = Math.Clamp(
            book.ProgressPercentage,
            0m,
            100m
        );

        int completedBlocks = (int)Math.Round(
            percentage / 100m *
            ProgressBarWidth
        );

        int remainingBlocks =
            ProgressBarWidth - completedBlocks;

        string completed =
            new('█', completedBlocks);

        string remaining =
            new('░', remainingBlocks);

        string content =
            $"[{LevelUpTheme.Success}]" +
            $"{completed}[/]" +
            $"[{LevelUpTheme.MutedText}]" +
            $"{remaining}[/] " +
            $"[{LevelUpTheme.Primary}]" +
            $"{percentage:0.##}%[/]";

        return new Markup(content);
    }
}