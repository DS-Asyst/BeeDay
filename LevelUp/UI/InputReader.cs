using System.Globalization;

namespace LevelUp.UI;

public class InputReader
{
    public string ReadRequiredString(string message)
    {
        while (true)
        {
            Console.Write(message);

            string? input = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            ShowError("O valor informado não pode ficar vazio.");
        }
    }

    public int ReadPositiveInteger(string message)
    {
        while (true)
        {
            Console.Write(message);

            string? input = Console.ReadLine();

            bool isValid = int.TryParse(
                input,
                out int value
            );

            if (isValid && value > 0)
            {
                return value;
            }

            ShowError("Informe um número inteiro maior que zero.");
        }
    }

    public decimal ReadDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);

            string? input = Console.ReadLine();

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

            ShowError("Informe um número decimal válido.");
        }
    }

    public int ReadOption(
        string message,
        int minimumOption,
        int maximumOption
    )
    {
        while (true)
        {
            Console.Write(message);

            string? input = Console.ReadLine();

            bool isValid = int.TryParse(
                input,
                out int option
            );

            if (
                isValid &&
                option >= minimumOption &&
                option <= maximumOption
            )
            {
                return option;
            }

            ShowError(
                $"Escolha uma opção entre " +
                $"{minimumOption} e {maximumOption}."
            );
        }
    }

    public bool ReadConfirmation(string message)
    {
        while (true)
        {
            Console.Write($"{message} (S/N): ");

            string? input = Console
                .ReadLine()?
                .Trim()
                .ToUpperInvariant();

            if (input == "S")
            {
                return true;
            }

            if (input == "N")
            {
                return false;
            }

            ShowError("Digite S para sim ou N para não.");
        }
    }

    public void WaitForContinue()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Pressione qualquer tecla para continuar..."
        );

        Console.ReadKey(intercept: true);
    }

    private static void ShowError(string message)
    {
        Console.WriteLine();
        Console.WriteLine($"Erro: {message}");
        Console.WriteLine();
    }
}