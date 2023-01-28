using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace SeelansTyres.Frontends.Mvc.Services;

public class DistributedCacheService : ICacheService
{
    private readonly ILogger<DistributedCacheService> logger;
    private readonly IDistributedCache cache;

    public DistributedCacheService(
        ILogger<DistributedCacheService> logger,
        IDistributedCache cache)
    {
        this.logger = logger;
        this.cache = cache;
    }

    public async Task<T?> RetrieveAsync<T>(string cacheKey)
    {
        logger.LogInformation(
            "Cache Service => Attempting to retrieve data using cache key {cacheKey}",
            cacheKey);

        var data = await cache.GetAsync(cacheKey);

        if (data is not null)
        {
            logger.LogInformation(
                "The cache does contain data linked to the cache key, deserializing to {modelType}",
                typeof(T).Name);

            return JsonSerializer.Deserialize<T>(data);
        }

        return default;
    }

    public async Task SetAsync<T>(string cacheKey, T data, int? slidingExpirationInMinutes, int? absoluteExpirationInMinutes)
    {
        logger.LogInformation(
            "Cache Service => Attempting to set cache key {cacheKey}",
            cacheKey);

        var cacheEntryOptions =
            new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpirationInMinutes ?? 2.0));

        if (absoluteExpirationInMinutes is not null)
        {
            cacheEntryOptions
                .SetAbsoluteExpiration(TimeSpan.FromMinutes((double)absoluteExpirationInMinutes));
        }

        try
        {
            await cache.SetAsync(cacheKey, JsonSerializer.SerializeToUtf8Bytes(data), cacheEntryOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Cache is unavailable");
        }
    }

    public async Task DeleteAsync(string cacheKey)
    {
        logger.LogInformation(
            "Cache Service => Attempting to remove data linked to cache key {cacheKey}",
            cacheKey);

        try
        {
            await cache.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Cache is unavailable");
        }
    }
}
