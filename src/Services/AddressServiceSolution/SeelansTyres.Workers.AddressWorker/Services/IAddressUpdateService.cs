using SeelansTyres.Libraries.Shared.Messages;

namespace SeelansTyres.Workers.AddressWorker.Services;

public interface IAddressUpdateService
{
    Task DeleteAsync(BaseMessage message);
}
