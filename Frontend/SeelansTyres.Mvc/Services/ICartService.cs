using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICartService
{
    List<CartItemModel> Retrieve();
    void CreateItem(CartItemModel newItem);
    void DeleteItem(Guid tyreId);
    void Delete();
}
