using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public DateTime Time { get; set; }
    [Required]
    public int TotalItems { get; set; }
    [Required]
    [Column(TypeName = "decimal")]
    public decimal TotalPrice { get; set; }
    [Required]
    public bool Delivered { get; set; }
    public Customer? Customer { get; set; }
    public Address? Address { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = null!;
}
