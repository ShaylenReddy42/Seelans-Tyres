using Microsoft.AspNetCore.Identity;
using SeelansTyres.Services.IdentityService.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SeelansTyres.Services.IdentityService.Services;

public class AdminAccountSeeder
{
    private readonly ILogger<AdminAccountSeeder> logger;
    private readonly IConfiguration configuration;
    private readonly RoleManager<IdentityRole<Guid>> roleManager;
    private readonly UserManager<Customer> userManager;

    public AdminAccountSeeder(
        ILogger<AdminAccountSeeder> logger,
        IConfiguration configuration,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<Customer> userManager)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.roleManager = roleManager;
        this.userManager = userManager;
    }

    public async Task CreateAdminAsync()
    {
        try
        {
            var administrators = await userManager!.GetUsersInRoleAsync("Administrator");

            if (administrators.Count is not 0)
            {
                return;
            }

            bool roleExists = await roleManager.RoleExistsAsync("Administrator");

            if (roleExists is false)
            {
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>
                    {
                        Name = "Administrator"
                    });
            }

            var admin = new Customer
            {
                FirstName = "Admin",
                LastName = "User",
                Email = configuration["AdminCredentials:Email"],
                UserName = configuration["AdminCredentials:Email"]
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Admin User"),
                new Claim(JwtRegisteredClaimNames.GivenName, "Admin"),
                new Claim(JwtRegisteredClaimNames.FamilyName, "User"),
                new Claim(ClaimTypes.Role, "Administrator")
            };

            await userManager.CreateAsync(admin, configuration["AdminCredentials:Password"]);
            await userManager.AddToRoleAsync(admin, "Administrator");
            await userManager.AddClaimsAsync(admin, claims);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "The database is unavailable");
        }
    }
}
