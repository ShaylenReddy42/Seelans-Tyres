using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Mvc.Models.External;

public class OrderItemModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int TyreId { get; set; }
    [Required]
    public string TyreName { get; set; } = string.Empty;
    [Required]
    public decimal TyrePrice { get; set; }
}