namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides functionality to work with images
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads an image if it's provided
    /// </summary>
    /// <param name="image">Image to upload</param>
    /// <param name="defaultImage">An image url to default to if no image is provided or upload fails</param>
    /// <returns>The url of the newly uploaded image</returns>
    Task<string> UploadAsync(IFormFile? image, string defaultImage);

    /// <summary>
    /// Deletes an image
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
    Task DeleteAsync(string imageUrl);
}
