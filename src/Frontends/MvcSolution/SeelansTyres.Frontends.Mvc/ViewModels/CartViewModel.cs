using SeelansTyres.Frontends.Mvc.Models; // CartItemModel

namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class CartViewModel
{
    public List<CartItemModel>? CartItems { get; set; }
    public int NumberOfAddresses { get; set; }
    public decimal TotalPrice => CartItems!.Sum(item => item.TotalItemPrice);
}
