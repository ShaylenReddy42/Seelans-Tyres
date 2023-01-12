using System.ComponentModel.DataAnnotations;

namespace SeelansTyres.Frontends.Mvc.Models;

public class MvcTyreModel : TyreModel
{
    public string? OriginalImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    [FileExtensions(Extensions = "jpg,jpeg,png")]
    public string? ImageFileName => Image is not null ? Image.FileName : null;
}
