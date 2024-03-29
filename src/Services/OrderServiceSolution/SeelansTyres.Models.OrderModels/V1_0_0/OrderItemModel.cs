﻿using System.ComponentModel.DataAnnotations; // Required

namespace SeelansTyres.Models.OrderModels.V1_0_0;

public class OrderItemModel
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    public Guid TyreId { get; set; }
    [Required]
    public string TyreName { get; set; } = string.Empty;
    [Required]
    public decimal TyrePrice { get; set; }
}