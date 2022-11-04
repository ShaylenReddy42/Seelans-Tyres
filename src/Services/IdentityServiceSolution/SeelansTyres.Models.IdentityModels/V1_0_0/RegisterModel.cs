using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Models.IdentityModels.V1_0_0;

public class RegisterModel
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    [MinLength(5)]
    public string Email { get; set; } = string.Empty;
    [RegularExpression(@"^\d{10}$")]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
