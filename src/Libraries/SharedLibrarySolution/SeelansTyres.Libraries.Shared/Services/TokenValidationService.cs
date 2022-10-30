using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SeelansTyres.Libraries.Shared.Messages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SeelansTyres.Libraries.Shared.Services;

public class TokenValidationService : ITokenValidationService
{
    private readonly ILogger<TokenValidationService> logger;
    private readonly HttpClient client;

    public TokenValidationService(
        ILogger<TokenValidationService> logger,
        HttpClient client)
    {
        this.logger = logger;
        this.client = client;
    }
    
    public async Task<bool> ValidateTokenAsync(BaseMessage message, string validIssuer, string validAudience)
    {
        DiscoveryDocumentResponse? discoveryDocument = null;

        try
        {
            discoveryDocument = await client.GetDiscoveryDocumentAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{announcement}: Could not retrieve the discovery document",
                "FAILED");
            
            return false;
        }

        var issuerSigningKeys = new List<SecurityKey>();

        var jsonWebKeys = discoveryDocument.KeySet.Keys;

        jsonWebKeys.ForEach(jwk =>
        {
            var rsaParameters = new RSAParameters
            {
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N)
            };

            var rsaSecurityKey = new RsaSecurityKey(rsaParameters)
            {
                KeyId = jwk.Kid
            };

            issuerSigningKeys.Add(rsaSecurityKey);
        });

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = validIssuer,
            ValidAudience = validAudience,
            IssuerSigningKeys = issuerSigningKeys,
            LifetimeValidator = (notBefore, expires, securityToken, tokenValidationParameters) =>
            {
                return expires!.Value.ToUniversalTime() > message.CreationTime.ToUniversalTime();
            }
        };

        try
        {
            logger.LogInformation("Attempting to validate the access token");
            
            _ = new JwtSecurityTokenHandler()
                .ValidateToken(
                    token: message.AccessToken,
                    validationParameters: tokenValidationParameters,
                    validatedToken: out _);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{announcement}: Attempt to validate token failed",
                "FAILED");

            return false;
        }
        
        return true;
    }
}
