using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Models;

public class CreateAddressModel
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string City { get; set; } = string.Empty;
    [Required]
    [StringLength(4, MinimumLength = 4)]
    public string PostalCode { get; set; } = string.Empty;
    public bool PreferredAddress { get; set; } = default;
}
