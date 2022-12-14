namespace SeelansTyres.Frontends.Mvc.Services;

public class LocalImageService : IImageService
{
    private readonly IWebHostEnvironment environment;
    private readonly ILogger<LocalImageService> logger;

    public LocalImageService(
        IWebHostEnvironment environment,
        ILogger<LocalImageService> logger)
    {
        this.environment = environment;
        this.logger = logger;
    }

    public async Task<string> UploadAsync(IFormFile? image, string defaultImage)
    {
        if (image is null)
        {
            logger.LogInformation(
                "{announcement}: Administrator didn't attempt to upload a new image",
                "NULL");

            return defaultImage;
        }

        logger.LogInformation("The administrator has chosen to upload a new image");

        var directory = Path.Combine(environment.WebRootPath, "images", "uploaded");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

        var filePath = Path.Combine(directory, fileName);

        Directory.CreateDirectory(directory);

        using var fileStream = new FileStream(filePath, FileMode.Create);

        await image.CopyToAsync(fileStream);

        filePath = $"/images/uploaded/{fileName}";

        return filePath;
    }
}
