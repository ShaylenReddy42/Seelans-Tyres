﻿using System.ComponentModel.DataAnnotations; // Required

namespace SeelansTyres.Models.OrderModels.V1_0_0;

public class OrderModel
{
    [Required]
    public int Id { get; set; }
    [Required]
    public DateTime OrderPlaced { get; set; }
    [Required]
    public decimal TotalPrice { get; set; }
    [Required]
    public bool Delivered { get; set; } = default;
    [Required]
    public string CustomerId { get; set; } = string.Empty;
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
    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
}
