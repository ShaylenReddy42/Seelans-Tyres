namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides functionality to send emails to customers
/// </summary>
public interface IMailService
{
    /// <summary>
    /// Emails a receipt to a customer using RazorLight to render the template
    /// </summary>
    /// <param name="order">The newly placed order</param>
    Task SendReceiptAsync(OrderModel order);

    /// <summary>
    /// Emails a token used to reset a customer's password and uses RazorLight to render the template
    /// </summary>
    /// <param name="customerEmail">The email address of the customer</param>
    /// <param name="firstName">The customer's first name, used in the email</param>
    /// <param name="lastName">The customer's last name, used in the email</param>
    /// <param name="token">The token needed by the customer to reset their password</param>
    /// <returns>A boolean indicating if the email was sent</returns>
    Task<bool> SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token);
}
