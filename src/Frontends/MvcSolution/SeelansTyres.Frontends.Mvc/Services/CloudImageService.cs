using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace SeelansTyres.Frontends.Mvc.Services;

public class CloudImageService : IImageService
{
    private readonly ILogger<CloudImageService> logger;
    private readonly IConfiguration configuration;

    public CloudImageService(
        ILogger<CloudImageService> logger,
        IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
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

        logger.LogInformation("Connecting to the Azure storage account");

        var blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("AzureStorageAccount"));

        logger.LogInformation("Retrieving a container client");

        var blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

        logger.LogInformation("Creating the container if it doesn't exist");

        await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

        logger.LogInformation("Retrieving a blob client");

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        using var fileStream = image.OpenReadStream();

        try
        {
            logger.LogInformation("Attempting to upload the blob to the storage account using a file stream");

            await blobClient.UploadAsync(
                fileStream,
                new BlobHttpHeaders
                {
                    ContentType = image.ContentType
                });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{announcement}: Attempt to upload the blob to the storage account failed",
                "FAILED");

            return defaultImage;
        }

        logger.LogInformation(
            "{announcement}: Attempt to upload the blob to the storage account completed successfully",
            "SUCCEEDED");

        return blobClient.Uri.AbsoluteUri;
    }
}
