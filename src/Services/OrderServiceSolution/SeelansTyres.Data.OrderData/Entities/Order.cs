﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.OrderData.Entities;

[Index(nameof(CustomerId), IsUnique = false, Name = "IX_Orders_CustomerId")]
[Index(nameof(AddressId), IsUnique = false, Name = "IX_Orders_AddressId")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public DateTime OrderPlaced { get; set; }
    [Required]
    [Column(TypeName = "decimal")]
    public decimal TotalPrice { get; set; }
    [Required]
    public bool Delivered { get; set; } = default;
    public Guid CustomerId { get; set; }
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
    [Required]
    public Guid AddressId { get; set; }
    [Required]
    public string AddressLine1 { get; set; } = string.Empty;
    [Required]
    public string AddressLine2 { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string PostalCode { get; set; } = string.Empty;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
