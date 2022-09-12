using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models;

public class PasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
