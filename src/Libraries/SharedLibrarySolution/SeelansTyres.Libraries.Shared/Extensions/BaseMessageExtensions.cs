using Microsoft.Extensions.Configuration;     // IConfiguration
using Microsoft.Extensions.Logging;           // ILogger
using SeelansTyres.Libraries.Shared.Messages; // BaseMessage
using SeelansTyres.Libraries.Shared.Services; // ITokenValidationService

namespace SeelansTyres.Libraries.Shared.Extensions;

public static class BaseMessageExtensions
{
    /// <summary>
    /// Provides an abstraction to validate an access token from the BaseMessage
    /// </summary>
    /// <remarks>
    ///     Exists purely to reduce code duplication and make client code more succinct
    /// </remarks>
    /// <param name="message">The message from the message bus</param>
    /// <param name="configuration">The injected instance of IConfiguration needed to extract the identity server url</param>
    /// <param name="logger">The injected instance of the logger</param>
    /// <param name="tokenValidationService">The injected instance of the token validation service</param>
    /// <param name="validAudience">Audience needed to validate the token, the value must align with the audience from the service that published the message</param>
    /// <param name="tokenIsValid">An output declaring if validation succeeded to continue to process the message</param>
    /// <returns>The original BaseMessage</returns>
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

        if (!tokenIsValid)
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
