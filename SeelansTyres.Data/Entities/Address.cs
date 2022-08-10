using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class Address
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
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
    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }
    public Guid CustomerId { get; set; }
}
