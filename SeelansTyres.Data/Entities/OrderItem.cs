using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class OrderItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int Quantity { get; set; }
    [ForeignKey("TyreId")]
    public Tyre? Tyre { get; set; }
    public int TyreId { get; set; }
    [ForeignKey("OrderId")]
    public Order? Order { get; set; }
    public int OrderId { get; set; }
}
