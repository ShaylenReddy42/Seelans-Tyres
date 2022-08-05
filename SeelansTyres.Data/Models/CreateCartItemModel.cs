using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Models;

public class CreateCartItemModel
{
    [Required]
    public int TyreId { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public string CartId { get; set; } = string.Empty;
}
