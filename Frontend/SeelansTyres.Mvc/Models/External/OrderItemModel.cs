namespace SeelansTyres.Mvc.Models.External;

public class OrderItemModel
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public int TyreId { get; set; }
    public string TyreName { get; set; } = string.Empty;
    public decimal TyrePrice { get; set; }
}