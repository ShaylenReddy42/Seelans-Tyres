﻿using System.ComponentModel.DataAnnotations;        // Required, StringLength, Range, MaxLength
using System.ComponentModel.DataAnnotations.Schema; // Column

namespace SeelansTyres.Models.TyresModels.V1_0_0;

public class TyreModel
{
    public Guid Id { get; set; }
    [Required]
    [StringLength(40, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(115, 355)]
    public int Width { get; set; }
    [Required]
    [Range(13, 19)]
    public int Ratio { get; set; }
    [Required]
    [Range(25, 75)]
    public int Diameter { get; set; }
    [Required]
    [MaxLength(40)]
    public string VehicleType { get; set; } = string.Empty;
    [Required]
    [Column(TypeName = "decimal")]
    public decimal Price { get; set; }
    public bool Available { get; set; } = true;
    public string ImageUrl { get; set; } = string.Empty;
    [Required]
    public int BrandId { get; set; }
    public BrandModel? Brand { get; set; }
}
