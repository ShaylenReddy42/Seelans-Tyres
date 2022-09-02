using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.ViewModels;

public class CartViewModel
{
    public List<CachedCartItemModel>? CartItems { get; set; }
    public int NumberOfAddresses { get; set; }
    public decimal TotalPrice => CartItems!.Sum(item => item.TotalItemPrice);
}
