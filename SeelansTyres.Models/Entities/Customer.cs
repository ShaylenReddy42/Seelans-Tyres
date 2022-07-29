using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Entities;

public class Customer : IdentityUser<Guid>
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    public ICollection<Address> Addresses { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = null!;
}
