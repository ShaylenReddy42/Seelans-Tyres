using SeelansTyres.Frontends.Mvc.Validation;
using System.ComponentModel.DataAnnotations;

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
