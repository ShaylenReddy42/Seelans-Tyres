using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;

namespace SeelansTyres.Mvc.Data;

public class SeelansTyresContext : IdentityDbContext<Customer, IdentityRole<Guid>, Guid>
{
    public SeelansTyresContext(
        DbContextOptions<SeelansTyresContext> options) : base(options)
    {

    }
}
