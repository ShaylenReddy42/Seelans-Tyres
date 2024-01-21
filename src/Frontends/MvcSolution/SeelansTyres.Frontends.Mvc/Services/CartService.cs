﻿using SeelansTyres.Frontends.Mvc.Models; // CartItemModel

namespace SeelansTyres.Frontends.Mvc.Services;

public class CartService(
    ILogger<CartService> logger,
    IHttpContextAccessor httpContextAccessor,
    ICacheService cacheService) : ICartService
{
    private readonly string cartId = httpContextAccessor.HttpContext?.Session.GetString("CartId")
                                  ?? throw new InvalidOperationException("The cartId cannot be null");

    public async Task CreateItemAsync(CartItemModel newItem)
    {
        logger.LogInformation(
            "Service => Attempting to add tyre {tyreId} to cart {cartId} with quantity {quantity}",
            newItem.TyreId, cartId, newItem.Quantity);
        
        var cart = await RetrieveAsync();

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

        await UpdateAsync(cart);
    }

    public async Task<List<CartItemModel>> RetrieveAsync()
    {
        logger.LogDebug(
            "Service => Attempting to retrieve cart {cartId}",
            cartId);

        var cart = await cacheService.RetrieveAsync<List<CartItemModel>>(cartId);

        if (cart is null)
        {
            logger.LogWarning(
                "Cart {cartId} doesn't exist in the cache. Adding it",
                cartId);
            
            cart = [];
            await UpdateAsync(cart);
        }

        return cart!;
    }

    private async Task UpdateAsync(List<CartItemModel> cart)
    {
        logger.LogInformation(
            "Service => Attempting to update cart {cartId}",
            cartId);
        
        await cacheService.SetAsync(cartId, cart, 30, 2 * 60);
    }

    public async Task DeleteItemAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Service => Attempting to remove tyre {tyreId} from cart {cartId}",
            tyreId, cartId);
        
        var cart = await RetrieveAsync();

        cart.Remove(cart.Single(item => item.TyreId == tyreId));

        await UpdateAsync(cart);
    }

    public async Task DeleteAsync()
    {
        logger.LogInformation(
            "Service => Attempting to remove cart {cartId} from the cache",
            cartId);
        
        await cacheService.DeleteAsync(cartId);
    }
}
