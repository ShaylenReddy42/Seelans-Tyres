using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace SeelansTyres.Services.IdentityService.Services;

public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
{
    private readonly ITokenValidator _validator;

    public TokenExchangeExtensionGrantValidator(ITokenValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
    private static string AccessTokenType => "urn:ietf:params:oauth:token-type:access_token";

    public async Task ValidateAsync(ExtensionGrantValidationContext context)
    {
        var requestedGrant = context.Request.Raw.Get("grant_type");
        var subjectToken = context.Request.Raw.Get("subject_token");
        var subjectTokenType = context.Request.Raw.Get("subject_token_type");

        var tokenValidationResult = await _validator.ValidateAccessTokenAsync(subjectToken);

        object[,] validationArray =
        {
            { string.IsNullOrWhiteSpace(requestedGrant) || requestedGrant != GrantType, "Invalid grant"              },
            { string.IsNullOrWhiteSpace(subjectToken),                                  "Subject token missing"      },
            { string.IsNullOrWhiteSpace(subjectTokenType),                              "Subject token type missing" },
            { subjectTokenType != AccessTokenType,                                      "Subject token type invalid" },
            { tokenValidationResult.IsError,                                            "Subject token invalid"      }
        };

        for (int i = 0; i < validationArray.GetLength(0); i++)
        {
            if (validationArray[i, 0] is true)
            {
                context.Result =
                    new GrantValidationResult(
                        TokenRequestErrors.InvalidRequest,
                        validationArray[i, 1].ToString());

                return;
            }
        }

        var subjectClaim = tokenValidationResult.Claims.SingleOrDefault(claim => claim.Type is "sub");

        if (subjectClaim is null)
        {
            context.Result =
                new GrantValidationResult(
                    TokenRequestErrors.InvalidRequest,
                    "Subject token must contain sub value");

            return;
        }

        context.Result = 
            new GrantValidationResult(
                subject: subjectClaim.Value, 
                authenticationMethod: "access_token",
                claims: tokenValidationResult.Claims);
    }
}


