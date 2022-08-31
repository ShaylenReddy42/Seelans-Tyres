using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ICartRepository
{
    Task CreateItemAsync(CartItem newCartItem);
    Task<CartItem?> RetrieveSingleItemAsync(int cartItemId);
    Task<IEnumerable<CartItem>> RetrieveCartAsync(string cartId);
    void DeleteItem(CartItem cartItem);
    void DeleteCart(IEnumerable<CartItem> cartItems);
    Task<bool> SaveChangesAsync();
}
