using System.ComponentModel.DataAnnotations;

namespace MediCareMS.Models.ViewModels.Auth;

public class InviteRegisterViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Please select your role")]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
