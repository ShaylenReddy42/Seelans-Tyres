using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Models;

public class TyreModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MinLength(3)]
    [MaxLength(40)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Range(115, 335)]
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
    [ForeignKey("BrandId")]
    public BrandModel? Brand { get; set; }
}
