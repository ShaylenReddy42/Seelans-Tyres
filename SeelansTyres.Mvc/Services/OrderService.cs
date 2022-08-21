using SeelansTyres.Data.Models;
using System.Net;

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

    public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync(Guid? customerId = null, bool notDeliveredOnly = false)
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
            logger.LogError(ex.Message);
            return new List<OrderModel>();
        }
    }

    public async Task<OrderModel?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            var response = await client.GetAsync($"api/orders/{orderId}");
            var order = await response.Content.ReadFromJsonAsync<OrderModel>();

            return order!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
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
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<int> PlaceNewOrderAsync(CreateOrderModel order)
    {
        try
        {
            var response = await client.PostAsync("api/orders", JsonContent.Create(order));

            return (await response.Content.ReadFromJsonAsync<OrderModel>())!.Id;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return 0;
        }
    }
}
