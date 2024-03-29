﻿using Azure.Storage.Blobs;        // BlobServiceClient
using Azure.Storage.Blobs.Models; // PublicAccessType, BlobHttpHeaders, DeleteSnapshotsOption

namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides a cloud implementation of the image service that uploads images to an Azure Storage account
/// </summary>
public class CloudImageService(
    ILogger<CloudImageService> logger,
    IConfiguration configuration) : IImageService
{
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

        logger.LogInformation("Attempting to delete the older image");

        await DeleteAsync(defaultImage);

        return blobClient.Uri.AbsoluteUri;
    }

    public async Task DeleteAsync(string imageUrl)
    {
        logger.LogInformation("Service => Attempting to delete image");

        var blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("AzureStorageAccount"));

        var blobContainerClient = blobServiceClient.GetBlobContainerClient("images");

        var blobContainerUri = blobContainerClient.Uri.AbsoluteUri;

        if (!imageUrl.StartsWith(blobContainerUri))
        {
            logger.LogWarning(
                "{announcement}: The image url is invalid and cannot be acted upon. It needs to start with '{blobContainerUri}'",
                "ABORTED", blobContainerUri);

            return;
        }

        imageUrl = imageUrl[(imageUrl.LastIndexOf('/') + 1) ..];

        await blobContainerClient.DeleteBlobIfExistsAsync(imageUrl, DeleteSnapshotsOption.IncludeSnapshots);

        logger.LogInformation(
            "{announcement}: Attempt to delete image completed successfully if it existed prior",
            "SUCCEEDED");
    }
}
