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
            ConsoleHelper.ShowHeader("Biblioteca");

            string option = inputReader.ReadSelection(
                "Escolha uma opção:",
                new[]
                {
                    "Cadastrar livro",
                    "Abrir livro",
                    "Meus livros",
                    "Voltar"
                },
                choice => choice
            );

            switch (option)
            {
                case "Cadastrar livro":
                    CreateBook();
                    inputReader.WaitForContinue();
                    break;

                case "Abrir livro":
                    OpenBook();
                    break;

                case "Meus livros":
                    ListBooks();
                    inputReader.WaitForContinue();
                    break;

                case "Voltar":
                    running = false;
                    break;
            }
        }
    }

    private void CreateBook()
    {
        ConsoleHelper.ShowHeader("Novo livro");
        inputReader.ShowCancellationHint();

        try
        {
            string title = inputReader.ReadRequiredStringOrCancel("Título:");
            string author = inputReader.ReadRequiredStringOrCancel("Autor:");
            int totalPages = inputReader.ReadPositiveIntegerOrCancel(
                "Total de páginas:"
            );

            PromptDecision decision = inputReader.ReadDecision(
                "Confirmar cadastro do livro?"
            );

            if (decision != PromptDecision.Yes)
            {
                ConsoleHelper.ShowInformation("Cadastro cancelado.");
                return;
            }

            BookModel book = bookService.CreateBook(
                title,
                author,
                totalPages
            );
            gameStateService.Save();

            string message = book.Status == BookStatus.Reading
                ? "Livro cadastrado e iniciado com sucesso."
                : "Livro cadastrado como bloqueado. Conclua ou arquive uma leitura em andamento para iniciá-lo.";

            ConsoleHelper.ShowSuccess(message);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new BookCard(book).Build());
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Cadastro do livro cancelado.");
        }
    }

    private void OpenBook()
    {
        IReadOnlyList<BookModel> books = bookService.GetAll();

        if (books.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhum livro foi cadastrado.");
            inputReader.WaitForContinue();
            return;
        }

        BookModel book = SelectBook(books);
        bool opened = true;

        while (opened)
        {
            ConsoleHelper.ShowHeader("Livro");
            AnsiConsole.Write(new BookCard(book).Build());
            AnsiConsole.WriteLine();

            List<string> actions = ["Editar"];

            if (book.Status == BookStatus.Locked)
            {
                actions.Add("Iniciar leitura");
            }

            if (book.Status == BookStatus.Reading)
            {
                actions.Add("Registrar progresso");
            }

            if (book.Status != BookStatus.Archived)
            {
                actions.Add("Arquivar");
            }

            actions.Add("Excluir");
            actions.Add("Voltar");

            string action = inputReader.ReadSelection(
                "Escolha uma ação:",
                actions,
                choice => choice
            );

            switch (action)
            {
                case "Editar":
                    EditBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Iniciar leitura":
                    StartBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Registrar progresso":
                    RecordProgress(book);
                    inputReader.WaitForContinue();
                    break;

                case "Arquivar":
                    ArchiveBook(book);
                    inputReader.WaitForContinue();
                    break;

                case "Excluir":
                    opened = !DeleteBook(book);
                    if (opened)
                    {
                        inputReader.WaitForContinue();
                    }
                    break;

                case "Voltar":
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
                "Novo título:"
            );
            string author = inputReader.ReadRequiredStringOrCancel(
                "Novo autor:"
            );
            int totalPages = inputReader.ReadPositiveIntegerOrCancel(
                "Novo total de páginas:"
            );

            if (!inputReader.ReadConfirmation("Salvar alterações?"))
            {
                ConsoleHelper.ShowInformation("Edição cancelada.");
                return;
            }

            bookService.UpdateBook(book, title, author, totalPages);
            gameStateService.Save();
            ConsoleHelper.ShowSuccess("Livro atualizado com sucesso.");
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Edição cancelada.");
        }
    }

    private void StartBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Iniciar a leitura de '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Ação cancelada.");
            return;
        }

        bookService.StartBook(book);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Leitura iniciada com sucesso.");
    }

    private void RecordProgress(BookModel book)
    {
        inputReader.ShowCancellationHint();

        try
        {
            AnsiConsole.MarkupLine(
                $"[grey]Última página registrada:[/] {book.CurrentPage}"
            );

            int currentPage = inputReader.ReadPositiveIntegerOrCancel(
                "Página atual:"
            );
            ReadingProgressResult result =
                readingWorkflowService.RecordProgress(book, currentPage);

            if (result.PagesRead == 0)
            {
                ConsoleHelper.ShowInformation(
                    "Nenhuma nova página foi registrada."
                );
                return;
            }

            ConsoleHelper.ShowSuccess($"{result.PagesRead} páginas registradas.");

            if (result.BookCompleted)
            {
                ConsoleHelper.ShowSuccess(
                    $"Livro concluído. Você ganhou {result.ExperienceEarned:0.##} XP."
                );
            }
        }
        catch (UserCancelledException)
        {
            ConsoleHelper.ShowInformation("Registro de leitura cancelado.");
        }
    }

    private void ArchiveBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Arquivar o livro '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Arquivamento cancelado.");
            return;
        }

        bookService.ArchiveBook(book);
        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Livro arquivado com sucesso.");
    }

    private bool DeleteBook(BookModel book)
    {
        if (!inputReader.ReadConfirmation(
            $"Excluir permanentemente o livro '{book.Title}'?"
        ))
        {
            ConsoleHelper.ShowInformation("Exclusão cancelada.");
            return false;
        }

        if (!bookService.DeleteBook(book.Id))
        {
            ConsoleHelper.ShowError("Não foi possível excluir o livro.");
            return false;
        }

        gameStateService.Save();
        ConsoleHelper.ShowSuccess("Livro excluído com sucesso.");
        return true;
    }

    private void ListBooks()
    {
        ConsoleHelper.ShowHeader("Meus livros");
        IReadOnlyList<BookModel> books = bookService.GetAll();

        if (books.Count == 0)
        {
            ConsoleHelper.ShowInformation("Nenhum livro foi cadastrado.");
            return;
        }

        AnsiConsole.Write(new BookTable(books).Build());
    }

    private BookModel SelectBook(IEnumerable<BookModel> books)
    {
        return inputReader.ReadSelection(
            "Selecione um livro:",
            books,
            book =>
                $"{book.Title} — {DisplayText.For(book.Status)} — " +
                $"{book.CurrentPage}/{book.TotalPages}"
        );
    }
}
