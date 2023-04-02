using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides functionality to work with a shopping cart and sits on top of a cache
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Adds an item to - or updates the quantity of an item in - the cart
    /// </summary>
    /// <param name="newItem">The model containing the tyre to be added to the cart</param>
    /// <returns></returns>
    Task CreateItemAsync(CartItemModel newItem);

    /// <summary>
    /// Gets all cart items for the user in the current session
    /// </summary>
    /// <remarks>
    /// If no cart is present, one is created and added to the cache and then returned as empty
    /// </remarks>
    /// <returns>A collection of cart items</returns>
    Task<List<CartItemModel>> RetrieveAsync();

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    /// <param name="tyreId">The id of the tyre that's linked to a cart item</param>
    Task DeleteItemAsync(Guid tyreId);

    /// <summary>
    /// Removes the entire cart for the user in the current session
    /// </summary>
    Task DeleteAsync();
}
