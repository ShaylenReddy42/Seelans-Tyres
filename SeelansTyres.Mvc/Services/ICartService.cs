using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICartService
{
    Task<IEnumerable<CartItemModel>> GetAllCartItemsAsync();
    Task<bool> AddItemToCartAsync(CreateCartItemModel item);
    Task<bool> RemoveItemFromCartAsync(int itemId);
    Task<bool> ClearCartAsync();
}
