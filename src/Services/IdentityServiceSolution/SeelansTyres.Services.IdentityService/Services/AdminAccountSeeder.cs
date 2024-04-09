using Microsoft.AspNetCore.Identity;                       // RoleManager, UserManager, IdentityRole
using SeelansTyres.Services.IdentityService.Data.Entities; // Customer
using System.Diagnostics;                                  // Stopwatch
using System.IdentityModel.Tokens.Jwt;                     // JwtRegisteredClaimNames
using System.Security.Claims;                              // Claim, ClaimTypes

namespace SeelansTyres.Services.IdentityService.Services;

public class AdminAccountSeeder
{
    private readonly ILogger<AdminAccountSeeder> logger;
    private readonly IConfiguration configuration;
    private readonly RoleManager<IdentityRole<Guid>> roleManager;
    private readonly UserManager<Customer> userManager;
    private readonly Stopwatch stopwatch = new();

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
        logger.LogInformation("Service => Attempting to create the administrator account");

        stopwatch.Start();
        try
        {
            var administrators = await userManager!.GetUsersInRoleAsync("Administrator");

            if (administrators.Count is not 0)
            {
                stopwatch.Stop();

                logger.LogWarning(
                    "{Announcement} ({stopwatchElapsedTime}ms): Administrator account already exists",
                    "ABORTED", stopwatch.ElapsedMilliseconds);
                
                return;
            }

            bool roleExists = await roleManager.RoleExistsAsync("Administrator");

            if (!roleExists)
            {
                logger.LogInformation(
                    "Creating role {role} because it doesn't exist", 
                    "Administrator");
                
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
                new(ClaimTypes.Name, "Admin User"),
                new(JwtRegisteredClaimNames.GivenName, "Admin"),
                new(JwtRegisteredClaimNames.FamilyName, "User"),
                new(ClaimTypes.Role, "Administrator")
            };

            await userManager.CreateAsync(admin, configuration["AdminCredentials:Password"]);
            await userManager.AddToRoleAsync(admin, "Administrator");
            await userManager.AddClaimsAsync(admin, claims);
        }
        catch (InvalidOperationException ex)
        {
            stopwatch.Stop();
            
            logger.LogError(
                ex,
                "{Announcement} ({stopwatchElapsedTime}ms): The database is unavailable",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw;
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({stopwatchElapsedTime}ms): Attempt to create an administrator account completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);
    }
}
