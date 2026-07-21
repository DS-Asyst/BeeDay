using System.ComponentModel.DataAnnotations;

namespace LevelUp.Web.Components.Features.Profile.Models;

public sealed class ProfileFormModel
{
    [Required(ErrorMessage = "Character name is required.")]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nickname is required.")]
    [StringLength(24, MinimumLength = 3, ErrorMessage = "Nickname must contain between 3 and 24 characters.")]
    [RegularExpression(@"^@?[A-Za-z0-9._-]+$", ErrorMessage = "Use only letters, numbers, dots, underscores or hyphens.")]
    public string Nickname { get; set; } = string.Empty;
}
