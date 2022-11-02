using FluentEmail.Core;
using SeelansTyres.Models.OrderModels.V1;
using System.Diagnostics;
using System.Reflection;

namespace SeelansTyres.Frontends.Mvc.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> logger;
    private readonly IFluentEmail email;
    private readonly Stopwatch stopwatch = new();

    public EmailService(
        ILogger<EmailService> logger,
        IFluentEmail email)
    {
        this.logger = logger;
        this.email = email;
    }
    
    public async Task SendReceiptAsync(OrderModel order)
    {
        logger.LogInformation(
            "Attempting to send a receipt for order {orderId} to customer {customerId}",
            order.Id, order.CustomerId);
        
        stopwatch.Start();
        try
        {
            _ = await email
                .To(order.Email, $"{order.FirstName} {order.LastName}")
                .Subject($"Your Seelan's Tyres Order #{order.Id}")
                .UsingTemplateFromEmbedded(
                    path: "SeelansTyres.Frontends.Mvc.Templates.Receipt.cshtml",
                    model: order,
                    assembly: Assembly.GetExecutingAssembly())
                .SendAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogWarning(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to send a receipt for order {orderId} to customer {customerId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, order.Id, order.CustomerId);
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to send a receipt for order {orderId} to customer {customerId} completed successful",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, order.Id, order.CustomerId);
    }

    public async Task<bool> SendResetPasswordTokenAsync(string customerEmail, string firstName, string lastName, string token)
    {
        logger.LogInformation(
            "Attempting to send a reset password token to customer with email {customerEmail}",
            "***REDACTED***");

        stopwatch.Start();
        try
        {
            _ = await email
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
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            logger.LogWarning(
                ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to send a reset password token to customer with email {customerEmail} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, "***REDACTED***");

            return false;
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to send a reset password token to customer with email {customerEmail} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, "***REDACTED***");

        return true;
    }
}
