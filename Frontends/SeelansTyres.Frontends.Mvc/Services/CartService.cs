using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

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
        logger.LogInformation(
            "Service => Attempting to add tyre {tyreId} to cart {cartId} with quantity {quantity}",
            newItem.TyreId, cartId, newItem.Quantity);
        
        var cart = Retrieve();

        var cartItem = cart.SingleOrDefault(item => item.TyreId == newItem.TyreId);

        if (cartItem is null)
        {
            cart.Add(newItem);
        }
        else
        {
            logger.LogInformation(
                "Tyre {tyreId} exists in the cart. Updating its quantity",
                newItem.TyreId);
            
            cart.Remove(cartItem);
            cartItem.Quantity += newItem.Quantity;
            cart.Add(cartItem);
        }

        Update(cart);
    }

    public List<CartItemModel> Retrieve()
    {
        logger.LogInformation(
            "Service => Attempting to retrieve cart {cartId}",
            cartId);
        
        if (cache.TryGetValue(cartId, out List<CartItemModel> cart) is false)
        {
            logger.LogInformation(
                "Cart {cartId} doesn't exist in the cache. Adding it",
                cartId);
            
            cart = new List<CartItemModel>();
            Update(cart);
        }

        return cart;
    }

    private void Update(List<CartItemModel> cart)
    {
        logger.LogInformation(
            "Service => Attempting to update cart {cartId}",
            cartId);
        
        var cacheEntryOptions =
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

        cache.Set(cartId, cart, cacheEntryOptions);
    }

    public void DeleteItem(Guid tyreId)
    {
        logger.LogInformation(
            "Service => Attempting to remove tyre {tyreId} from cart {cartId}",
            tyreId, cartId);
        
        var cart = Retrieve();

        cart.Remove(cart.Single(item => item.TyreId == tyreId));

        Update(cart);
    }

    public void Delete()
    {
        logger.LogInformation(
            "Service => Attempting to remove cart {cartId} from the cache",
            cartId);
        
        cache.Remove(cartId);
    }
}
