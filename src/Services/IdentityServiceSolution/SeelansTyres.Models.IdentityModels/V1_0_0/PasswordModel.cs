using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Models.IdentityModels.V1_0_0;

public class PasswordModel
{
    [Required]
    public string Password { get; set; } = string.Empty;
}
