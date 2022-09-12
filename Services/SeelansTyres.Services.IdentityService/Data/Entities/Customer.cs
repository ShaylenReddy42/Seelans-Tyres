using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Services.IdentityService.Data.Entities;

public class Customer : IdentityUser<Guid>
{
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string FirstName { get; set; } = string.Empty;
    [RegularExpression("^[a-zA-Z]{2,40}$")]
    public string LastName { get; set; } = string.Empty;
}
