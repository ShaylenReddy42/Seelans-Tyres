using Microsoft.EntityFrameworkCore;                // Index
using System.ComponentModel.DataAnnotations;        // Key, Required, StringLength, RegularExpression
using System.ComponentModel.DataAnnotations.Schema; // DatabaseGenerated

namespace SeelansTyres.Data.AddressData.Entities;

[Index(nameof(CustomerId), IsUnique = false, Name = "IX_Addresses_CustomerId")]
public class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;
    [RegularExpression(@"^\d{4}$")]
    public string PostalCode { get; set; } = string.Empty;
    public bool PreferredAddress { get; set; } = default;
    public Guid CustomerId { get; set; }
}
