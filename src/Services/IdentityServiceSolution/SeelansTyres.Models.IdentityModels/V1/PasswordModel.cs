using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Models.IdentityModels.V1;

public class PasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
