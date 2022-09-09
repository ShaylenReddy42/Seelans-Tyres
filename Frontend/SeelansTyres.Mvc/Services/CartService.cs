using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public class CartService : ICartService
{
    private readonly ILogger<CartService> logger;
    private readonly string cartId;
    private readonly IMemoryCache cache;

    public CartService(
        ILogger<CartService> logger,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache) =>
            (this.logger, cartId, this.cache) = (logger, httpContextAccessor.HttpContext!.Session.GetString("CartId")!, cache);
    
    public void CreateItem(CartItemModel newItem)
    {
        var cart = Retrieve();

        var cartItem = cart.SingleOrDefault(item => item.TyreId == newItem.TyreId);

        if (cartItem is null)
        {
            cart.Add(newItem);
        }
        else
        {
            cart.Remove(cartItem);
            cartItem.Quantity += newItem.Quantity;
            cart.Add(cartItem);
        }

        Update(cart);
    }

    public void Delete() =>
        cache.Remove(cartId);

    public void DeleteItem(Guid tyreId)
    {
        var cart = Retrieve();

        cart.Remove(cart.Single(item => item.TyreId == tyreId));

        Update(cart);
    }

    public List<CartItemModel> Retrieve()
    {
        List<CartItemModel> cart;
        
        if (cache.TryGetValue(cartId, out cart) is false)
        {
            cart = new List<CartItemModel>();
            Update(cart);
        }

        return cart;
    }

    private void Update(List<CartItemModel> cart)
    {
        var cacheEntryOptions =
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

        cache.Set(cartId, cart, cacheEntryOptions);
    }
}
