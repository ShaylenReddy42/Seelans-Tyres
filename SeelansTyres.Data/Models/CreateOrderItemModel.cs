﻿using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Data.Models;

public class CreateOrderItemModel
{
    [Required]
    public int Quantity { get; set; }
    [Required]
    public int TyreId { get; set; }
}