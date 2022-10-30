using SeelansTyres.Libraries.Shared.Messages;

namespace SeelansTyres.Workers.OrderWorker.Services;

public interface IOrderUpdateService
{
    Task DeleteAccountAsync(BaseMessage message);
    Task UpdateAccountAsync(BaseMessage message);
    Task UpdateTyreAsync(BaseMessage message);
}
