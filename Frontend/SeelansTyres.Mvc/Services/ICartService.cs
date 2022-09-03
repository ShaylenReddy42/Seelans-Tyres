using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICartService
{
    List<CachedCartItemModel> Retrieve();
    void CreateItem(CachedCartItemModel newItem);
    void DeleteItem(Guid tyreId);
    void Delete();
}
