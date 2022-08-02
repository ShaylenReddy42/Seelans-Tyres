namespace SeelansTyres.Data.Models;

public class TyreModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Ratio { get; set; }
    public int Diameter { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Available { get; set; } = true;
    public string ImageUrl { get; set; } = string.Empty;
    public BrandModel? Brand { get; set; }
}
