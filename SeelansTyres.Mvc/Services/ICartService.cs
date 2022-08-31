using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICartService
{
    Task<IEnumerable<CartItemModel>> RetrieveCartAsync();
    Task<bool> CreateItemAsync(CreateCartItemModel item);
    Task<bool> DeleteItemAsync(int itemId);
    Task<bool> DeleteCartAsync();
}
