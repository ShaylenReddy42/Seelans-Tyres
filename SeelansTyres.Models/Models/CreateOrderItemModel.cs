using SeelansTyres.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Models;

public class CreateOrderItemModel
{
    [Required]
    public int Quantity { get; set; }
    public Tyre? Tyre { get; set; }
}