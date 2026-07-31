using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Components.Features.Wallets.Models;

public sealed class WalletTagFormModel
{
    [Required, StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Use a valid hexadecimal color.")]
    public string Color { get; set; } = "#7A4FCB";
}
