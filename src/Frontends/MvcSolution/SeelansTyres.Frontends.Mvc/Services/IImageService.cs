namespace SeelansTyres.Frontends.Mvc.Services;

public interface IImageService
{
    Task<string> UploadAsync(IFormFile? image, string defaultImage);
}
