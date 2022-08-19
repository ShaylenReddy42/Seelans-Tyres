using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public class CartService : ICartService
{
    private readonly HttpClient client;
    private readonly string cartId;
    private readonly ILogger<CartService> logger;

    public CartService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CartService> logger) =>
            (this.client, cartId, this.logger) = (client, httpContextAccessor.HttpContext!.Session.GetString("CartId")!, logger);

    public async Task<bool> AddItemToCartAsync(CreateCartItemModel item)
    {
        try
        {
            await client.PostAsync("api/cart", JsonContent.Create(item));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> ClearCartAsync()
    {
        try
        {
            await client.DeleteAsync($"api/cart/{cartId}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<IEnumerable<CartItemModel>> GetAllCartItemsAsync()
    {
        try
        {
            var response = await client.GetAsync($"api/cart/{cartId}");
            var cartItems = await response.Content.ReadFromJsonAsync<IEnumerable<CartItemModel>>();

            return cartItems!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return new List<CartItemModel>();
        }
    }

    public async Task<bool> RemoveItemFromCartAsync(int itemId)
    {
        try
        {
            await client.DeleteAsync($"api/cart/{cartId}/items/{itemId}");
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }
}
