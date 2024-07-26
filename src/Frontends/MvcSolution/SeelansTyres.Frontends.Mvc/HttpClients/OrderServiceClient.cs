using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants

namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class OrderServiceClient(
    HttpClient client,
    ILogger<OrderServiceClient> logger) : IOrderServiceClient
{
    public async Task<OrderModel?> CreateAsync(OrderModel order)
    {
        logger.LogInformation("Service => Attempting to place a new order");

        try
        {
            using var response = await client.PostAsync("api/orders", JsonContent.Create(order));
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to place a new order completed successfully",
                LoggerConstants.SucceededAnnouncement);

            return await response.Content.ReadFromJsonAsync<OrderModel>();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to place a new order was unsuccessful",
                LoggerConstants.FailedAnnouncement);

            return null;
        }
    }

    public async Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false)
    {
        var loggerForPlaceholderValue = customerId is not null ? " for customer " : "";
        var loggerCustomerIdPlaceholderValue = customerId is not null ? customerId.ToString() : "";
        var loggerExceptDeliveredPlaceholderValue = notDeliveredOnly ? " except delivered ones" : "";


        logger.LogInformation(
            "Service => Attempting to retrieve all orders{For}{CustomerId}{ExceptDelivered}",
            loggerForPlaceholderValue, loggerCustomerIdPlaceholderValue, loggerExceptDeliveredPlaceholderValue);

        try
        {
            var response = customerId switch
            {
                null => notDeliveredOnly switch
                {
                    true => await client.GetAsync($"api/orders?notDeliveredOnly=true"),
                    _    => await client.GetAsync($"api/orders")
                },
                _ => notDeliveredOnly switch
                {
                    true => await client.GetAsync($"api/orders?customerId={customerId}&notDeliveredOnly=true"),
                    _    => await client.GetAsync($"api/orders?customerId={customerId}")
                }
            };
            response.EnsureSuccessStatusCode();

            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderModel>>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve all orders{For}{CustomerId}{ExceptDelivered} completed successfully with {OrdersCount} order(s)",
                LoggerConstants.SucceededAnnouncement, loggerForPlaceholderValue, loggerCustomerIdPlaceholderValue, loggerExceptDeliveredPlaceholderValue, orders?.Count() ?? 0);

            return orders ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve all orders{For}{CustomerId}{ExceptDelivered}",
                LoggerConstants.FailedAnnouncement, loggerForPlaceholderValue, loggerCustomerIdPlaceholderValue, loggerExceptDeliveredPlaceholderValue);

            return [];
        }
    }

    public async Task<OrderModel?> RetrieveSingleAsync(int orderId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve order {OrderId}",
            orderId);

        try
        {
            using var response = await client.GetAsync($"api/orders/{orderId}");
            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderModel>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve order {OrderId} completed successfully",
                LoggerConstants.SucceededAnnouncement, orderId);

            return order;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve order {OrderId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, orderId);

            return null;
        }
    }

    public async Task<bool> MarkOrderAsDeliveredAsync(int orderId)
    {
        logger.LogInformation(
            "Service => Attempting to mark order {OrderId} as delivered",
            orderId);

        try
        {
            using var response = await client.PutAsync($"api/orders/{orderId}?delivered=true", null);
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to mark order {OrderId} as delivered completed successfully",
                LoggerConstants.SucceededAnnouncement, orderId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to mark order {OrderId} as delivered was unsuccessful",
                LoggerConstants.FailedAnnouncement, orderId);

            return false;
        }
    }
}
