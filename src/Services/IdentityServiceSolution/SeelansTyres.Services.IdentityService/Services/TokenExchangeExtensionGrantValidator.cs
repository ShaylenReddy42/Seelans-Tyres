using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace SeelansTyres.Services.IdentityService.Services;

public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
{
    private readonly ITokenValidator _validator;
    private readonly ILogger<TokenExchangeExtensionGrantValidator> logger;

    public TokenExchangeExtensionGrantValidator(
        ITokenValidator validator,
        ILogger<TokenExchangeExtensionGrantValidator> logger)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
    private static string AccessTokenType => "urn:ietf:params:oauth:token-type:access_token";

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        logger.LogInformation("Token exchange validation started");
        
        var requestedGrant = context.Request.Raw.Get("grant_type");
        var subjectToken = context.Request.Raw.Get("subject_token");
        var subjectTokenType = context.Request.Raw.Get("subject_token_type");

        var tokenValidationResult = await _validator.ValidateAccessTokenAsync(subjectToken);

        var subjectClaim = tokenValidationResult.Claims.SingleOrDefault(claim => claim.Type is "sub");

        object[,] validationArray =
        {
            { requestedGrant != GrantType,                 "Invalid grant"                        },
            { string.IsNullOrWhiteSpace(subjectToken),     "Subject token missing"                },
            { string.IsNullOrWhiteSpace(subjectTokenType), "Subject token type missing"           },
            { subjectTokenType != AccessTokenType,         "Subject token type invalid"           },
            { tokenValidationResult.IsError,               "Subject token invalid"                },
            { subjectClaim is null,                        "Subject token must contain sub value" }
        };

        for (int i = 0; i < validationArray.GetLength(0); i++)
        {
            if (validationArray[i, 0] is true)
            {
                logger.LogWarning(
                    "{announcement}: Token exchange validation failed with reason '{validationFailureReason}'",
                    "FAILED", validationArray[i, 1]);
                
                context.Result =
                    new GrantValidationResult(
                        TokenRequestErrors.InvalidRequest,
                        validationArray[i, 1].ToString());

                return;
            }
        }

        logger.LogInformation(
            "{announcement}: Token exchange validation completed successfully",
            "SUCCEEDED");

        context.Result = 
            new GrantValidationResult(
                subject: subjectClaim!.Value, 
                authenticationMethod: "access_token",
                claims: tokenValidationResult.Claims);
    }
}


