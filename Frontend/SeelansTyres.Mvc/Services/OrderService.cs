using SeelansTyres.Mvc.Models.External;

namespace SeelansTyres.Mvc.Services;

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
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {httpContextAccessor.HttpContext!.Session.GetString("ApiAuthToken")}");
    }

    public async Task<IEnumerable<OrderModel>> RetrieveAllAsync(Guid? customerId = null, bool notDeliveredOnly = false)
    {
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

            var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderModel>>();

            return orders!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return new List<OrderModel>();
        }
    }

    public async Task<OrderModel?> RetrieveSingleAsync(int orderId)
    {
        try
        {
            var response = await client.GetAsync($"api/orders/{orderId}");
            var order = await response.Content.ReadFromJsonAsync<OrderModel>();

            return order!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return null;
        }
    }

    public async Task<bool> MarkOrderAsDeliveredAsync(int orderId)
    {
        try
        {
            _ = await client.PutAsync($"api/orders/{orderId}?delivered=true", new StringContent(""));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return false;
        }
    }

    public async Task<OrderModel?> CreateAsync(CreateOrderModel order)
    {
        try
        {
            var response = await client.PostAsync("api/orders", JsonContent.Create(order));

            return await response.Content.ReadFromJsonAsync<OrderModel>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return null;
        }
    }
}
