using SeelansTyres.Libraries.Shared.Messages;

namespace SeelansTyres.Libraries.Shared.Services;

public interface ITokenValidationService
{
    Task<bool> ValidateTokenAsync(BaseMessage message, string validIssuer, string validAudience);
}
