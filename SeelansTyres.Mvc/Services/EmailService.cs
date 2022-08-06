using FluentEmail.Core;
using SeelansTyres.Data.Models;
using System.Reflection;

namespace SeelansTyres.Mvc.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> logger;
    private readonly IFluentEmail email;

    public EmailService(
        ILogger<EmailService> logger,
        IFluentEmail email)
    {
        this.logger = logger;
        this.email = email;
    }
    
    public async Task SendReceiptAsync(OrderModel order)
    {
       await email
            .To(order.Customer!.Email, $"{order.Customer.FirstName} {order.Customer.LastName}")
            .Subject($"Your Seelan's Tyres Order #{order.Id}")
            .UsingTemplateFromEmbedded(
                path: "SeelansTyres.Mvc.Templates.Receipt.cshtml", 
                model: order,
                assembly: Assembly.GetExecutingAssembly())
            .SendAsync();
    }
}
