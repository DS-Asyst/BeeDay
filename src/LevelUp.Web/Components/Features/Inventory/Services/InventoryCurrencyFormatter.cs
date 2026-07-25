using System.Globalization;

namespace LevelUp.Web.Components.Features.Inventory.Services;

public static class InventoryCurrencyFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");

    public static string Format(decimal value) => value.ToString("C", Culture);

    public static string FormatSigned(decimal value, bool isIncome)
        => $"{(isIncome ? "+" : "-")}{Format(Math.Abs(value))}";
}
