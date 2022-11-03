namespace SeelansTyres.Frontends.Mvc.Services;

public class OrderService : IOrderService
{
    private readonly HttpClient client;
    private readonly ILogger<OrderService> logger;

    public OrderService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<OrderService> logger)
    {
        this.client = client;
        this.logger = logger;

        if (httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated is true)
        {
            var roleClaim = httpContextAccessor.HttpContext!.User.Claims.SingleOrDefault(claim => claim.Type.EndsWith("role"));

            if (roleClaim is not null && roleClaim.Value is "Administrator")
            {
                logger.LogInformation("Currently logged in user is an administrator. Adding custom X-User-Role header to all requests");

                client.DefaultRequestHeaders.Add("X-User-Role", "Administrator");
            }
        }
    }

    public async Task<OrderModel?> CreateAsync(OrderModel order)
    {
        logger.LogInformation("Service => Attempting to place a new order");

        try
        {
            var response = await client.PostAsync("api/orders", JsonContent.Create(order));

            logger.LogInformation(
                "{announcement}: Attempt to place a new order completed successfully",
                "SUCCEEDED");

            return await response.Content.ReadFromJsonAsync<OrderModel>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return null;
        }
    }

    public async Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve all orders{for}{customerId}{exceptDelivered}",
            customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly is true ? " except delivered ones" : "");

        try
        {
            var response = customerId switch
            {
                null => notDeliveredOnly switch
                {
                    true  => await client.GetAsync($"api/orders?notDeliveredOnly=true"),
                    false => await client.GetAsync($"api/orders")
                },
                _    => notDeliveredOnly switch
                {
                    true  => await client.GetAsync($"api/orders?customerId={customerId}&notDeliveredOnly=true"),
                    false => await client.GetAsync($"api/orders?customerId={customerId}")
                }
            };
            response.EnsureSuccessStatusCode();

            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderModel>>();

            logger.LogInformation(
                "{announcement}: Attempt to retrieve all orders{for}{customerId}{exceptDelivered} completed successfully with {ordersCount} order(s)",
                "SUCCEEDED", customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", 
                notDeliveredOnly is true ? " except delivered ones" : "", orders!.Count());

            return orders!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return new List<OrderModel>();
        }
    }

    public async Task<OrderModel?> RetrieveSingleAsync(int orderId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve order {orderId}",
            orderId);

        try
        {
            var response = await client.GetAsync($"api/orders/{orderId}");
            response.EnsureSuccessStatusCode();

            var order = await response.Content.ReadFromJsonAsync<OrderModel>();

            logger.LogInformation(
                "{announcement}: Attempt to retrieve order {orderId} completed successfully",
                "SUCCEEDED", orderId);

            return order!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
				ex, 
				"{announcement}: The API is unavailable",
				"FAILED");

			return null;
        }
    }

    public async Task<bool> MarkOrderAsDeliveredAsync(int orderId)
    {
        logger.LogInformation(
            "Service => Attempting to mark order {orderId} as delivered",
            orderId);

        try
        {
            _ = await client.PutAsync($"api/orders/{orderId}?delivered=true", new StringContent(""));

            logger.LogInformation(
                "Attempt to mark order {orderId} as delivered completed successfully",
                orderId);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return false;
        }
    }
}
