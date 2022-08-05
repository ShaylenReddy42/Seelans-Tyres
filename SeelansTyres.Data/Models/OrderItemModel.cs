namespace SeelansTyres.Data.Models;

public class OrderItemModel
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public TyreModel? Tyre { get; set; }
}