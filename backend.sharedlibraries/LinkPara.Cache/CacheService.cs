using Microsoft.Extensions.Caching.Memory;

namespace LinkPara.Cache;

public class CacheService : ICacheService
{
    private const int DefaultExpirationInMinutes = 10;

    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Add<T>(string key, T value, int? expirationInMinutes = null)
    {
        _cache.Set(key, value, TimeSpan.FromMinutes(expirationInMinutes ?? DefaultExpirationInMinutes));
    }

    public T Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T value)
            ? value
            : default;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> task, int? expirationInMinutes = null)
    {
        var cacheEntry = await
            _cache.GetOrCreateAsync<T>(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationInMinutes ?? DefaultExpirationInMinutes);
                return await task();
            });

        return cacheEntry;
    }

    public Task<bool> ContainsKeyAsync<T>(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out T _));
    }
}