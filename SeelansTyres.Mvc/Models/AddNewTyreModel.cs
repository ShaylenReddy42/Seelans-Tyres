﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Mvc.Models;

public class AddNewTyreModel
{
    [Required]
    [MinLength(3)]
    [MaxLength(40)]
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
    public IFormFile? Image { get; set; }
    [FileExtensions(Extensions = "jpg,jpeg,png")]
    public string? ImageFileName => Image is not null ? Image.FileName : null;
    [Required]
    public int BrandId { get; set; }
}
