using System.Globalization;
using Spectre.Console;

namespace LevelUp.UI;

public class InputReader
{
    public string ReadRequiredString(string message)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            )
            .PromptStyle("white")
            .ValidationErrorMessage(
                "[red]The value cannot be empty.[/]"
            )
            .Validate(input =>
            {
                return string.IsNullOrWhiteSpace(input)
                    ? ValidationResult.Error(
                        "[red]The value cannot be empty.[/]"
                    )
                    : ValidationResult.Success();
            })
        ).Trim();
    }

    public int ReadPositiveInteger(string message)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>(
                $"[yellow]{Markup.Escape(message)}[/]"
            )
            .PromptStyle("white")
            .ValidationErrorMessage(
                "[red]Enter an integer greater than zero.[/]"
            )
            .Validate(value =>
            {
                return value > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error(
                        "[red]Enter an integer greater than zero.[/]"
                    );
            })
        );
    }

    public decimal ReadDecimal(string message)
    {
        while (true)
        {
            string input = AnsiConsole.Ask<string>(
                $"[yellow]{Markup.Escape(message)}[/]"
            );

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
        return AnsiConsole.Confirm(
            $"[yellow]{Markup.Escape(message)}[/]",
            defaultValue: false
        );
    }

    public T ReadSelection<T>(
        string title,
        IEnumerable<T> choices,
        Func<T, string> converter
    )
        where T : notnull
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<T>()
                .Title(
                    $"[yellow]{Markup.Escape(title)}[/]"
                )
                .PageSize(10)
                .MoreChoicesText(
                    "[grey](Move up and down to see more options)[/]"
                )
                .AddChoices(choices)
                .UseConverter(converter)
        );
    }

    public void WaitForContinue()
    {
        ConsoleHelper.WaitForContinue();
    }
}