using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models.External;

public class CreateOrderItemModel
{
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int TyreId { get; set; }
    [Required]
    public string TyreName { get; set; } = string.Empty;
    [Required]
    public decimal TyrePrice { get; set; }
}