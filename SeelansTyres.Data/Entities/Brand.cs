using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Entities;

public class Brand
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = string.Empty;
    public ICollection<Tyre> Tyres { get; set; } = null!;
}
