using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.ViewModels;

public class CartViewModel
{
    public IEnumerable<CartItemModel>? CartItems { get; set; }
    public int NumberOfAddresses { get; set; }
    public decimal TotalPrice => CartItems!.Sum(item => item.TotalItemPrice);
}
