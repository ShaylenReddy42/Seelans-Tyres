using Microsoft.AspNetCore.Identity;
using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

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

            var admin = new Customer()
            {
                FirstName = "Admin",
                LastName = "User",
                Email = configuration["AdminCredentials:Email"],
                UserName = configuration["AdminCredentials:Email"]
            };

            await userManager.CreateAsync(admin, configuration["AdminCredentials:Password"]);
            await userManager.AddToRoleAsync(admin, "Administrator");
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex.Message);
        }
    }
}
