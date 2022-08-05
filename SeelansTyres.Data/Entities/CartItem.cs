using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class CartItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [ForeignKey("TyreId")]
    public Tyre? Tyre { get; set; }
    public int TyreId { get; set; }
    public int Quantity { get; set; }
    [Required]
    public string CartId { get; set; } = string.Empty;
}
