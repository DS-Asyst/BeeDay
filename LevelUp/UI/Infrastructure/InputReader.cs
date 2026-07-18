using System.Globalization;
using LevelUp.UI.Infrastructure;
using Spectre.Console;

namespace LevelUp.UI;

public class InputReader
{
    public const string CancellationCommand = "cancel";

    public string ReadRequiredString(string message)
    {
        return ReadRequiredStringCore(message, allowCancellation: false);
    }

    public string ReadRequiredStringOrCancel(string message)
    {
        return ReadRequiredStringCore(message, allowCancellation: true);
    }

    public int ReadPositiveInteger(string message)
    {
        return ReadPositiveIntegerCore(message, allowCancellation: false);
    }

    public int ReadPositiveIntegerOrCancel(string message)
    {
        return ReadPositiveIntegerCore(message, allowCancellation: true);
    }

    public decimal ReadDecimal(string message)
    {
        return ReadDecimalCore(message, allowCancellation: false);
    }

    public decimal ReadDecimalOrCancel(string message)
    {
        return ReadDecimalCore(message, allowCancellation: true);
    }

    public decimal ReadPositiveDecimalOrCancel(string message)
    {
        while (true)
        {
            decimal value = ReadDecimalCore(
                message,
                allowCancellation: true
            );

            if (value > 0)
            {
                return value;
            }

            ConsoleHelper.ShowError(
                "Enter a value greater than zero."
            );
        }
    }

    public DateTime ReadDateOrCancel(string message)
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            );

            ThrowIfCancelled(input, allowCancellation: true);

            if (DateTime.TryParseExact(
                input.Trim(),
                "dd/MM/yyyy",
                CultureInfo.GetCultureInfo("pt-BR"),
                DateTimeStyles.None,
                out DateTime value
            ))
            {
                return value.Date;
            }

            ConsoleHelper.ShowError(
                "Enter a valid date in dd/MM/yyyy format."
            );
        }
    }

    public int ReadOption(
        string message,
        int minimumOption,
        int maximumOption
    )
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>(
                $"[yellow]{Markup.Escape(message)}[/]"
            )
            .PromptStyle("white")
            .ValidationErrorMessage(
                $"[red]Choose an option between " +
                $"{minimumOption} and {maximumOption}.[/]"
            )
            .Validate(option =>
            {
                bool isValid =
                    option >= minimumOption &&
                    option <= maximumOption;

                return isValid
                    ? ValidationResult.Success()
                    : ValidationResult.Error(
                        $"[red]Choose an option between " +
                        $"{minimumOption} and {maximumOption}.[/]"
                    );
            })
        );
    }

    public bool ReadConfirmation(string message)
    {
        return ReadSelection(
            message,
            new[] { "Yes", "No" },
            choice => choice
        ) == "Yes";
    }

    public PromptDecision ReadDecision(string message)
    {
        string answer = ReadSelection(
            message,
            new[] { "Yes", "No", "Cancel" },
            choice => choice
        );

        return answer switch
        {
            "Yes" => PromptDecision.Yes,
            "No" => PromptDecision.No,
            _ => PromptDecision.Cancel
        };
    }

    public T ReadSelection<T>(
        string title,
        IEnumerable<T> choices,
        Func<T, string> converter
    )
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(choices);
        ArgumentNullException.ThrowIfNull(converter);

        SelectionPrompt<T> prompt =
            new SelectionPrompt<T>()
                .Title(Markup.Escape(title))
                .AddChoices(choices)
                .UseConverter(
                    choice => Markup.Escape(
                        converter(choice)
                    )
                );

        return AnsiConsole.Prompt(prompt);
    }

    public void ShowCancellationHint()
    {
        AnsiConsole.MarkupLine(
            $"[grey]Type [bold]{CancellationCommand}[/] " +
            "at any time to cancel.[/]"
        );
        AnsiConsole.WriteLine();
    }

    public void WaitForContinue()
    {
        ConsoleHelper.WaitForContinue();
    }

    public static bool IsCancellationCommand(string? value)
    {
        return string.Equals(
            value?.Trim(),
            CancellationCommand,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string ReadRequiredStringCore(
        string message,
        bool allowCancellation
    )
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            ).Trim();

            ThrowIfCancelled(input, allowCancellation);

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            ConsoleHelper.ShowError(
                "The value cannot be empty."
            );
        }
    }

    private static int ReadPositiveIntegerCore(
        string message,
        bool allowCancellation
    )
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            );

            ThrowIfCancelled(input, allowCancellation);

            if (int.TryParse(input, out int value) && value > 0)
            {
                return value;
            }

            ConsoleHelper.ShowError(
                "Enter an integer greater than zero."
            );
        }
    }

    private static decimal ReadDecimalCore(
        string message,
        bool allowCancellation
    )
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            );

            ThrowIfCancelled(input, allowCancellation);

            bool isValid = decimal.TryParse(
                input,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal value
            );

            if (isValid)
            {
                return value;
            }

            ConsoleHelper.ShowError(
                "Enter a valid decimal number."
            );
        }
    }

    private static void ThrowIfCancelled(
        string input,
        bool allowCancellation
    )
    {
        if (allowCancellation && IsCancellationCommand(input))
        {
            throw new UserCancelledException();
        }
    }
}
