using Microsoft.AspNetCore.Identity;         // IdentityUser
using System.ComponentModel.DataAnnotations; // RegularExpression

namespace SeelansTyres.Services.IdentityService.Data.Entities;

public class Customer : IdentityUser<Guid>
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
}
