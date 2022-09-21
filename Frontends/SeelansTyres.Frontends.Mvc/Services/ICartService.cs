using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface ICartService
{
    List<CartItemModel> Retrieve();
    void CreateItem(CartItemModel newItem);
    void DeleteItem(Guid tyreId);
    void Delete();
}
