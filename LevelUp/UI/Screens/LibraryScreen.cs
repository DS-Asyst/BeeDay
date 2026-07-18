using LevelUp.Domain.Books;
using LevelUp.Services.Books;
using LevelUp.Services.Persistence;
using LevelUp.Services.Workflows;
using LevelUp.UI.Components.Book;
using LevelUp.UI.Infrastructure;
using Spectre.Console;
using BookModel = LevelUp.Domain.Books.Book;

namespace LevelUp.UI;

public sealed class LibraryScreen
{
    private readonly BookService bookService;
    private readonly ReadingWorkflowService readingWorkflowService;
    private readonly GameStateService gameStateService;
    private readonly InputReader inputReader;

    public LibraryScreen(
        BookService bookService,
        ReadingWorkflowService readingWorkflowService,
        GameStateService gameStateService,
        InputReader inputReader
    )
    {
        this.bookService = bookService;
        this.readingWorkflowService = readingWorkflowService;
        this.gameStateService = gameStateService;
        this.inputReader = inputReader;
    }

    public void Show()
    {
        bool running = true;

        while (running)
        {
            ConsoleHelper.ShowHeader("Library");

            string option = inputReader.ReadSelection(
                "Choose an option:",
                new[]
                {
                    "Add Book",
                    "Open Book",
                    "My Books",
                    "Back"
                },
                choice => choice
            );

            switch (option)
            {
                case "Add Book":
                    CreateBook();
                    inputReader.WaitForContinue();
                    break;

                case "Open Book":
                    OpenBook();
                    break;

                case "My Books":
                    ListBooks();
                    inputReader.WaitForContinue();
                    break;

                case "Back":
                    running = false;
                    break;
            }
        }
    }

    private void CreateBook()
    {
        ConsoleHelper.ShowHeader("New Book");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel("Title:");
            string author = inputReader.ReadRequiredStringOrCancel("Author:");
            int totalPages = inputReader.ReadPositiveIntegerOrCancel(
                "Total pages:"
            );

            PromptDecision decision = inputReader.ReadDecision(
                "Add this book?"
            );

            if (decision != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Book creation canceled.");
                return;
            }

            BookModel book = bookService.CreateBook(
                title,
                author,
                totalPages
            );
            gameStateService.Save();

            string message = book.Status == BookStatus.Reading
                ? "Book created and started successfully."
                : "Book created as locked. Complete or archive the current reading to start it.";

            ConsoleHelper.ShowSuccess(message);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new BookCard(book).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Book creation canceled.");
        }
    }

    private void OpenBook()
    {
        IReadOnlyList<BookModel> books = bookService.GetAll();

        if (books.Count == 0)
        {
            ConsoleHelper.ShowInformation("No books have been created.");
            inputReader.WaitForContinue();
            return;
        }

        BookModel book = SelectBook(books);
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Book");
            AnsiConsole.Write(new BookCard(book).Build());
            AnsiConsole.WriteLine();

            List<string> actions = ["Edit"];

            if (book.Status == BookStatus.Locked)
            {
                actions.Add("Start Reading");
            }

            if (book.Status == BookStatus.Reading)
            {
                actions.Add("Record Progress");
            }

            if (book.Status != BookStatus.Archived)
            {
                actions.Add("Archive");
            }

            actions.Add("Delete");
            actions.Add("Back");

            string action = inputReader.ReadSelection(
                "Choose an action:",
                actions,
                choice => choice
            );

            switch (action)
            {
                case "Edit":
                    EditBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Start Reading":
                    StartBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Record Progress":
                    RecordProgress(book);
                    inputReader.WaitForContinue();
                    break;

                case "Archive":
                    ArchiveBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Delete":
                    opened = !DeleteBook(book);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Back":
                    opened = false;
                    break;
            }
        }
    }

    private void EditBook(BookModel book)
    {
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel(
                "New title:"
            );
            string author = inputReader.ReadRequiredStringOrCancel(
                "New author:"
            );
            int totalPages = inputReader.ReadPositiveIntegerOrCancel(
                "New total pages:"
            );

            if (!inputReader.ReadConfirmation("Save changes?"))
            {
                ConsoleHelper.ShowInformation("Edit cancelled.");
                return;
            }

            bookService.UpdateBook(book, title, author, totalPages);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Book updated successfully.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Edit cancelled.");
        }
    }

    private void StartBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Start reading '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Action cancelled.");
            return;
        }

        bookService.StartBook(book);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Reading started successfully.");
    }

    private void RecordProgress(BookModel book)
    {
        inputReader.ShowCancellationHint();

        try
        {
            AnsiConsole.MarkupLine(
                $"[grey]Last recorded page:[/] {book.CurrentPage}"
            );

            int currentPage = inputReader.ReadPositiveIntegerOrCancel(
                "Current page:"
            );
            ReadingProgressResult result =
                readingWorkflowService.RecordProgress(book, currentPage);

            if (result.PagesRead == 0)
            {
                ConsoleHelper.ShowInformation(
                    "No new pages were recorded."
                );
                return;
            }

            ConsoleHelper.ShowSuccess($"{result.PagesRead} pages recorded.");

            if (result.BookCompleted)
            {
                ConsoleHelper.ShowSuccess(
                    $"Book completed. You earned {result.ExperienceEarned:0.##} XP."
                );
            }
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Reading update canceled.");
        }
    }

    private void ArchiveBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Archive the book '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Archiving canceled.");
            return;
        }

        bookService.ArchiveBook(book);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Book archived successfully.");
    }

    private bool DeleteBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Permanently delete the book '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Deletion cancelled.");
            return false;
        }

        if (!bookService.DeleteBook(book.Id))
        {
            ConsoleHelper.ShowError("The book could not be deleted.");
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Book deleted successfully.");
        return true;
    }

    private void ListBooks()
    {
        ConsoleHelper.ShowHeader("My Books");
        IReadOnlyList<BookModel> books = bookService.GetAll();

        if (books.Count == 0)
        {
            ConsoleHelper.ShowInformation("No books have been created.");
            return;
        }

        AnsiConsole.Write(new BookTable(books).Build());
    }

    private BookModel SelectBook(IEnumerable<BookModel> books)
    {
        return inputReader.ReadSelection(
            "Select a book:",
            books,
            book =>
                $"{book.Title} — {DisplayText.For(book.Status)} — " +
                $"{book.CurrentPage}/{book.TotalPages}"
        );
    }
}
