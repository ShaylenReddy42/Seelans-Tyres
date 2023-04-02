namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides a local implementation for the image service that uploads images to wwwroot/images/uploaded
/// </summary>
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

        logger.LogInformation("Attempting to delete the older image");

        await DeleteAsync(defaultImage);

        return filePath;
    }

    public Task DeleteAsync(string imageUrl)
    {
        logger.LogInformation("Service => Attempting to delete image");
        
        if (imageUrl.StartsWith("/images/uploaded/") is false)
        {
            logger.LogWarning(
                "{announcement}: The image url is invalid and cannot be acted upon. It needs to start with '/images/uploaded/'",
                "ABORTED");
            
            return Task.CompletedTask;
        }

        imageUrl = imageUrl[1 ..].Replace('/', Path.DirectorySeparatorChar);

        imageUrl = Path.Combine(environment.WebRootPath, imageUrl);

        if (File.Exists(imageUrl) is false)
        {
            logger.LogWarning(
                "{announcement}: The image doesn't exist on disk. Exiting early",
                "ABORTED");

            return Task.CompletedTask;
        }

        File.Delete(imageUrl);

        logger.LogInformation(
            "{announcement}: Attempt to delete image completed successfully",
            "SUCCEEDED");

        return Task.CompletedTask;
    }
}
