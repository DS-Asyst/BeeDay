using System.ComponentModel.DataAnnotations;

namespace BeeDay.Web.Components.Features.Account.Models;

public sealed class ProfileFormModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must contain between 2 and 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;
}
