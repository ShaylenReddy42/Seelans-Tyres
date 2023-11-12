using SeelansTyres.Libraries.Shared.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Libraries.Shared.HttpClients;

/// <summary>
/// Provides functionality to validate an access token
/// </summary>
public interface ITokenValidationService
{
    /// <summary>
    /// Used to validate an access token after receiving an update from the message bus
    /// </summary>
    /// <param name="message">The model containing the access token</param>
    /// <param name="validIssuer">The url of the identity service that issued the token</param>
    /// <param name="validAudience">The audience value from the microservice that published the update</param>
    /// <returns></returns>
    Task<bool> ValidateTokenAsync(BaseMessage message, string validIssuer, string validAudience);
}
