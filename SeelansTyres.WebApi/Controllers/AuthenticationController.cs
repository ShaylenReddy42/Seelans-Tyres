using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> logger;
    private readonly UserManager<Customer> userManager;
    private readonly IConfiguration configuration;

    public AuthenticationController(
        ILogger<AuthenticationController> logger,
        UserManager<Customer> userManager,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.userManager = userManager;
        this.configuration = configuration;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginModel login)
    {
        var customer = await userManager.FindByEmailAsync(login.UserName);

        if (customer is null)
        {
            logger.LogWarning("Customer with id {customerId} does not exist", login.UserName);
            return NotFound();
        }
        
        var validSignIn = await userManager.CheckPasswordAsync(customer, login.Password);

        if (validSignIn is false)
        {
            logger.LogWarning("Invalid login attempt!");
            return Unauthorized();
        }

        if (configuration["Token:Key"].Length < 32)
        {
            throw new ArgumentOutOfRangeException(
                paramName: @"configuration[""Token:Key""]", 
                actualValue: configuration["Token:Key"].Length, 
                message: "Key must be at least 32 bits long");
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Token:Key"]));

        var signingCredentials = new SigningCredentials(
            securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, configuration["Token:Issuer"]),
            new Claim(JwtRegisteredClaimNames.Aud, configuration["Token:Audience"]),
            new Claim(JwtRegisteredClaimNames.GivenName, customer.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, customer.LastName)
        };

        if (await userManager.IsInRoleAsync(customer, "Administrator") is true)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

        var jwtToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: signingCredentials);

        var jwtTokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        
        return Ok(jwtTokenToReturn);
    }
}
