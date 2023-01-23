using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface ICartService
{
    Task CreateItemAsync(CartItemModel newItem);
    Task<List<CartItemModel>> RetrieveAsync();
    Task DeleteItemAsync(Guid tyreId);
    Task DeleteAsync();
}
