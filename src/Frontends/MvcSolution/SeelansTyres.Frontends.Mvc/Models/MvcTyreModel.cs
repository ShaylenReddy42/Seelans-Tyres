using SeelansTyres.Frontends.Mvc.Validation; // FileSizeLimit
using System.ComponentModel.DataAnnotations; // FileExtensions

namespace SeelansTyres.Frontends.Mvc.Models;

public class MvcTyreModel : TyreModel
{
    public string? OriginalImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    [FileExtensions(Extensions = "jpg,jpeg,png")]
    public string? ImageFileName => Image?.FileName;
    [FileSizeLimit(3)]
    public long? ImageSize => Image?.Length;
}
