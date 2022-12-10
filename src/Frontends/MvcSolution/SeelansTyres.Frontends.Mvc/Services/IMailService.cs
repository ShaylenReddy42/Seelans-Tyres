namespace SeelansTyres.Frontends.Mvc.Services;

public interface IMailService
{
    Task SendReceiptAsync(OrderModel order);
    Task<bool> SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token);
}
