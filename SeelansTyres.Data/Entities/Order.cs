using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public DateTime OrderPlaced { get; set; } = DateTime.Now;
    [Required]
    [Column(TypeName = "decimal")]
    public decimal TotalPrice { get; set; }
    [Required]
    public bool Delivered { get; set; } = default;
    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }
    public Guid CustomerId { get; set; }
    [ForeignKey("AddressId")]
    public Address? Address { get; set; }
    public int AddressId { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
