using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ICartRepository
{
    Task AddItemToCartAsync(CartItem cartItem);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(string cartId);
    void RemoveItemFromCart(CartItem cartItem);
    void RemoveCartById(IEnumerable<CartItem> cartItems);
    Task<bool> SaveChangesAsync();
}
