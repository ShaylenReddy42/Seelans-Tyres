using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models;

public class RegisterModel
{
    [Required]
    [StringLength(maximumLength: 40, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(maximumLength: 40, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MinLength(5)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
