using SeelansTyres.Frontends.Mvc.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface ICartService
{
    void CreateItem(CartItemModel newItem);
    List<CartItemModel> Retrieve();
    void DeleteItem(Guid tyreId);
    void Delete();
}
