using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Entities;

public class Customer : IdentityUser<Guid>
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
