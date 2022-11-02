using SeelansTyres.Models.OrderModels.V1;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface IEmailService
{
    Task SendReceiptAsync(OrderModel order);
    Task<bool> SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token);
}
