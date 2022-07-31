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
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
