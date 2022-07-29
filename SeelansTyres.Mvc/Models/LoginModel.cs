using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public bool RememberMe { get; set; } = default;
}
