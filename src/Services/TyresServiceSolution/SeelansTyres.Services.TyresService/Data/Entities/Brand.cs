using System.ComponentModel.DataAnnotations;        // Key, Required, MinLength
using System.ComponentModel.DataAnnotations.Schema; // DatabaseGenerated

namespace SeelansTyres.Services.TyresService.Data.Entities;

public class Brand
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public ICollection<Tyre> Tyres { get; set; } = [];
}
