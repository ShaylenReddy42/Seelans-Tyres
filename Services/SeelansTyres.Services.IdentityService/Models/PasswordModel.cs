using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Services.IdentityService.Models;

public class PasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
