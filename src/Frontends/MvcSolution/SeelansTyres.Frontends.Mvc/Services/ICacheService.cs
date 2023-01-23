namespace SeelansTyres.Frontends.Mvc.Services;

public interface ICacheService
{
    public Task<T?> RetrieveAsync<T>(string cacheKey);
    public Task SetAsync<T>(string cacheKey, T data, int? slidingExpirationInMinutes, int? absoluteExpirationInMinutes);
    public Task DeleteAsync(string cacheKey);
}
