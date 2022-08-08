using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IEmailService
{
    Task SendReceiptAsync(OrderModel order);
    Task SendResetPasswordTokenAsync(string email, string firstName, string lastName, string token);
}
