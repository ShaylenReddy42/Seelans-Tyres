using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICartService
{
    List<CachedCartItemModel> Retrieve();
    void CreateItem(CachedCartItemModel newItem);
    void DeleteItem(int tyreId);
    void Delete();
}
