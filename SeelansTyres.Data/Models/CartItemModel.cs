namespace SeelansTyres.Data.Models;

public class CartItemModel
{
    public int Id { get; set; }
    public TyreModel? Tyre { get; set; }
    public int Quantity { get; set; }
    public decimal TotalItemPrice => Tyre!.Price * Quantity;
    public string CartId { get; set; } = string.Empty;
}
