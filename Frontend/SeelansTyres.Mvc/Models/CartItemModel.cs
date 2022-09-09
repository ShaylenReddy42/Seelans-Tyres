namespace SeelansTyres.Mvc.Models;

public class CartItemModel
{
    public Guid TyreId { get; set; }
    public string TyreName { get; set; } = string.Empty;
    public decimal TyrePrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalItemPrice => TyrePrice * Quantity;
}
