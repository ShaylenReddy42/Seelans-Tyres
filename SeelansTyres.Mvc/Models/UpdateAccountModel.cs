using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models;

public class UpdateAccountModel
{
    [Required]
    [StringLength(maximumLength: 40, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [StringLength(maximumLength: 40, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string PhoneNumber { get; set; } = string.Empty;

}
