using Microsoft.Extensions.Caching.Memory; // IMemoryCache, MemoryCacheEntryOptions
using System.Text.Json;                    // JsonSerializer

namespace SeelansTyres.Frontends.Mvc.Services;

/// <summary>
/// Provides an in-memory implementation of the cache service
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly ILogger<InMemoryCacheService> logger;
    private readonly IMemoryCache cache;

    public InMemoryCacheService(
        ILogger<InMemoryCacheService> logger,
        IMemoryCache cache)
    {
        this.logger = logger;
        this.cache = cache;
    }

    public Task<T?> RetrieveAsync<T>(string cacheKey)
    {
        logger.LogInformation(
            "Cache Service => Attempting to retrieve data using cache key {cacheKey}",
            cacheKey);
        
        cache.TryGetValue(cacheKey, out byte[]? data);

        if (data is not null)
        {
            logger.LogInformation(
                "The cache does contain data linked to the cache key, deserializing to {modelType}",
                typeof(T).Name);

            return Task.FromResult(JsonSerializer.Deserialize<T>(data));
        }

        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string cacheKey, T data, int? slidingExpirationInMinutes, int? absoluteExpirationInMinutes)
    {
        logger.LogInformation(
            "Cache Service => Attempting to set cache key {cacheKey}",
            cacheKey);

        var cacheEntryOptions =
            new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpirationInMinutes ?? 2.0));

        if (absoluteExpirationInMinutes is not null)
        {
            cacheEntryOptions
                .SetAbsoluteExpiration(TimeSpan.FromMinutes((double)absoluteExpirationInMinutes));
        }

        cache.Set(cacheKey, JsonSerializer.SerializeToUtf8Bytes(data), cacheEntryOptions);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string cacheKey)
    {
        logger.LogInformation(
            "Cache Service => Attempting to remove data linked to cache key {cacheKey}",
            cacheKey);

        cache.Remove(cacheKey);

        return Task.CompletedTask;
    }
}
