using FluentEmail.Core;

namespace SeelansTyres.Frontends.Mvc.Services;

public class MailService(
    ILogger<MailService> logger,
    IFluentEmailFactory fluentEmailFactory) : IMailService
{
    private readonly Stopwatch stopwatch = new();

    public async Task SendReceiptAsync(OrderModel order)
    {
        logger.LogInformation(
            "Attempting to send a receipt for order {OrderId} to customer {CustomerId}",
            order.Id, order.CustomerId);
        
        stopwatch.Start();
        try
        {
            var email = fluentEmailFactory.Create();
            
            await email
                .To(order.Email, $"{order.FirstName} {order.LastName}")
                .Subject($"Your Seelan's Tyres Order #{order.Id}")
                .UsingTemplateFromEmbedded(
                    path: "SeelansTyres.Frontends.Mvc.Templates.Receipt.cshtml",
                    model: order,
                    assembly: Assembly.GetExecutingAssembly())
                .SendAsync();

            stopwatch.Stop();

            logger.LogInformation(
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to send a receipt for order {OrderId} to customer {CustomerId} completed successfully",
                "SUCCEEDED", stopwatch.ElapsedMilliseconds, order.Id, order.CustomerId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogWarning(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to send a receipt for order {OrderId} to customer {CustomerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, order.Id, order.CustomerId);
        }
    }

    public async Task<bool> SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token)
    {
        logger.LogInformation(
            "Attempting to send a reset password token to customer with email {CustomerEmail}",
            "***REDACTED***");

        stopwatch.Start();
        try
        {
            var email = fluentEmailFactory.Create();
            
            await email
                .To(customerEmail, $"{firstName} {lastName}")
                .Subject("Seelan's Tyres: Your Reset Password Token")
                .UsingTemplateFromEmbedded(
                    path: "SeelansTyres.Frontends.Mvc.Templates.VerificationToken.cshtml",
                    model: new
                    {
                        FirstName = firstName,
                        Token = token
                    },
                    assembly: Assembly.GetExecutingAssembly())
                .SendAsync();

            stopwatch.Stop();

            logger.LogInformation(
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to send a reset password token to customer with email {CustomerEmail} completed successfully",
                "SUCCEEDED", stopwatch.ElapsedMilliseconds, "***REDACTED***");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogWarning(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to send a reset password token to customer with email {CustomerEmail} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, "***REDACTED***");

            return false;
        }
        
        return true;
    }
}
