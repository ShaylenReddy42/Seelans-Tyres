namespace SeelansTyres.Mvc.Services;

public class LocalImageService : IImageService
{
    private readonly IWebHostEnvironment environment;

    public LocalImageService(IWebHostEnvironment environment) =>
        this.environment = environment;
    
    public async Task<string> UploadAsync(IFormFile? image, string defaultImage)
    {
        if (image is null)
        {
            return defaultImage;
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

        var filePath = 
            Path.Combine(
                environment.WebRootPath,
                "images",
                fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        filePath = $"/images/{fileName}";

        return filePath;
    }
}
