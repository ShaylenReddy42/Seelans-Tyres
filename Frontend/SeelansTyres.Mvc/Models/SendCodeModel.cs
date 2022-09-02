using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models;

public class SendCodeModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
