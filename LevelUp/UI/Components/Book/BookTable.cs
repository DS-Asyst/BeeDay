using LevelUp.Domain.Books;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI.Components.Book;

public sealed class BookTable
{
    private readonly IEnumerable<LevelUp.Domain.Books.Book> books;

    public BookTable(IEnumerable<LevelUp.Domain.Books.Book> books)
    {
        this.books = books;
    }

    public Table Build()
    {
        Table table = new Table()
            .Border(TableBorder.Rounded)
            .Expand();

        table.AddColumn("Livro");
        table.AddColumn("Autor");
        table.AddColumn("Status");
        table.AddColumn("Página");
        table.AddColumn("Progresso");

        foreach (LevelUp.Domain.Books.Book book in books)
        {
            table.AddRow(
                Markup.Escape(book.Title),
                Markup.Escape(book.Author),
                DisplayText.For(book.Status),
                $"{book.CurrentPage}/{book.TotalPages}",
                $"{book.ProgressPercentage:0.##}%"
            );
        }

        return table;
    }
}
