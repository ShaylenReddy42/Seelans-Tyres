﻿using Microsoft.AspNetCore.Identity;                       // RoleManager, UserManager, IdentityRole
using SeelansTyres.Libraries.Shared.Constants;             // LoggerConstants
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

    private const string FullAdminRoleName = "Administrator";

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
            var administrators = await userManager!.GetUsersInRoleAsync(FullAdminRoleName);

            if (administrators.Count is not 0)
            {
                stopwatch.Stop();

                logger.LogWarning(
                    "{Announcement} ({StopwatchElapsedTime}ms): Administrator account already exists",
                    "ABORTED", stopwatch.ElapsedMilliseconds);
                
                return;
            }

            bool roleExists = await roleManager.RoleExistsAsync(FullAdminRoleName);

            if (!roleExists)
            {
                logger.LogInformation(
                    "Creating role {Role} because it doesn't exist", 
                    FullAdminRoleName);
                
                await roleManager.CreateAsync(
                    new IdentityRole<Guid>
                    {
                        Name = FullAdminRoleName
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
                new(ClaimTypes.Role, FullAdminRoleName)
            };

            await userManager.CreateAsync(admin, configuration["AdminCredentials:Password"]);
            await userManager.AddToRoleAsync(admin, FullAdminRoleName);
            await userManager.AddClaimsAsync(admin, claims);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): The database is unavailable",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to create an administrator account completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds);
    }
}
