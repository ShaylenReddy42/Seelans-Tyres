using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Services.OrderService.Data.Entities;

[Index(nameof(TyreId), IsUnique = false, Name = "IX_OrderItems_TyreId")]
public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public Guid TyreId { get; set; }
    [Required]
    public string TyreName { get; set; } = string.Empty;
    [Required]
    [Column(TypeName = "decimal")]
    public decimal TyrePrice { get; set; }
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
    public int OrderId { get; set; }
}
