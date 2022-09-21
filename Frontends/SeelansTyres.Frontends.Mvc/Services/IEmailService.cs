using SeelansTyres.Frontends.Mvc.Models.External;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface IEmailService
{
    Task SendReceiptAsync(OrderModel order);
    Task SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token);
}
