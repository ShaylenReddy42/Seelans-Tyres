using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Models;

public class CreateAddressModel
{
    [Required]
    [MinLength(5)]
    [MaxLength(100)]
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    [Required]
    [MinLength(4)]
    [MaxLength(4)]
    public string PostalCode { get; set; } = string.Empty;
    public bool PreferredAddress { get; set; } = default;
}
