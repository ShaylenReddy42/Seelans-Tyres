using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Mvc.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SeelansTyres.Mvc.Services;

public class TokenService : ITokenService
{
    private readonly ISession session;
    private readonly IConfiguration configuration;

    public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) =>
        (session, this.configuration) = (httpContextAccessor.HttpContext!.Session, configuration);
    
    public void GenerateApiAuthToken(Customer customer, bool isAdmin)
    {
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
            new Claim(JwtRegisteredClaimNames.Aud, "AddressService"),
            new Claim(JwtRegisteredClaimNames.Aud, "OrderService"),
            new Claim(JwtRegisteredClaimNames.Aud, "TyresService"),
            new Claim(JwtRegisteredClaimNames.GivenName, customer.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, customer.LastName)
        };

        if (isAdmin is true)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        }

        var jwtToken = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: signingCredentials);

        session.SetString("ApiAuthToken", new JwtSecurityTokenHandler().WriteToken(jwtToken));
    }
}
