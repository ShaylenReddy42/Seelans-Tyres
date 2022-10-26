using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Frontends.Mvc.Models;

public class PasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
