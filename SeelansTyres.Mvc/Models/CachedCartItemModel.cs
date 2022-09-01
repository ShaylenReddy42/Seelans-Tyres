namespace SeelansTyres.Mvc.Models;

public class CachedCartItemModel
{
    public int TyreId { get; set; }
    public string TyreName { get; set; } = string.Empty;
    public decimal TyrePrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalItemPrice => TyrePrice * Quantity;
}
