using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeelansTyres.Libraries.Shared.Messages;
using SeelansTyres.Libraries.Shared.Services;

namespace SeelansTyres.Libraries.Shared.Extensions;

public static class BaseMessageExtensions
{
    public static BaseMessage ValidateTokenFromBaseMessage(
        this BaseMessage message,
        IConfiguration configuration,
        ILogger logger,
        ITokenValidationService tokenValidationService,
        string validAudience,
        out bool tokenIsValid)
    {
        logger.LogInformation("Worker => Attempting to validate the access token");

        tokenIsValid =
            tokenValidationService.ValidateTokenAsync(
                message!,
                configuration["IdentityServer"]!,
                validAudience).Result;

        if (tokenIsValid is false)
        {
            logger.LogError(
                "{announcement}: Attempt to validate the access token was unsuccessful",
                "FAILED");
        }
        else
        {
            logger.LogInformation(
                "{announcement}: Attempt to validate the access token completed successfully",
                "SUCCEEDED");
        }

        return message;
    }
}
