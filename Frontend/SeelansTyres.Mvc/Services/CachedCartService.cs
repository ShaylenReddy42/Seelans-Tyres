using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public class CachedCartService : ICartService
{
    private readonly ILogger<CachedCartService> logger;
    private readonly string cartId;
    private readonly IMemoryCache cache;

    public CachedCartService(
        ILogger<CachedCartService> logger,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache) =>
            (this.logger, cartId, this.cache) = (logger, httpContextAccessor.HttpContext!.Session.GetString("CartId")!, cache);
    
    public void CreateItem(CachedCartItemModel newItem)
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

    public List<CachedCartItemModel> Retrieve()
    {
        List<CachedCartItemModel> cart;
        
        if (cache.TryGetValue(cartId, out cart) is false)
        {
            cart = new List<CachedCartItemModel>();
            Update(cart);
        }

        return cart;
    }

    private void Update(List<CachedCartItemModel> cart)
    {
        var cacheEntryOptions =
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

        cache.Set(cartId, cart, cacheEntryOptions);
    }
}
