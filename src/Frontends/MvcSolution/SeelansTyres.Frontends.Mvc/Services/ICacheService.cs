namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides functionality to work with a cache
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves data from the cache and deserialized it to type of T
    /// </summary>
    /// <typeparam name="T">The model to deserialize to</typeparam>
    /// <param name="cacheKey">The key initially used to store the data</param>
    /// <returns>A deserialized model if cache key exists in the cache, else null</returns>
    public Task<T?> RetrieveAsync<T>(string cacheKey);

    /// <summary>
    /// Adds data to the cache using a specified cache key
    /// </summary>
    /// <typeparam name="T">The model to be serialized into a utf8 bytes version of the model in json</typeparam>
    /// <param name="cacheKey">The lookup key</param>
    /// <param name="data">The model to be stored in the cache</param>
    /// <param name="slidingExpirationInMinutes">The minimum time the data stays in the cache without interaction before being removed</param>
    /// <param name="absoluteExpirationInMinutes">The maximum time the data is allowed to stay in the cache</param>
    public Task SetAsync<T>(string cacheKey, T data, int? slidingExpirationInMinutes, int? absoluteExpirationInMinutes);
    /// <summary>
    /// Removes data from the cache that's tied to the specified cache key
    /// </summary>
    /// <param name="cacheKey">The lookup key</param>
    public Task DeleteAsync(string cacheKey);
}
