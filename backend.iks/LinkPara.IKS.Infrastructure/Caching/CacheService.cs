using Microsoft.Extensions.Caching.Memory;

namespace LinkPara.IKS.Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Add<T>(string key, T value, DateTime? absoluteExpiration = null)
    {
        DateTime offset;

        if (absoluteExpiration.HasValue)
        {
            offset = absoluteExpiration.Value;
        }
        else
        {
            offset = DateTime.MaxValue;
        }

        _cache.Set(key, value, offset);
    }

    public T Get<T>(string key) where T : class
    {
        if (_cache.TryGetValue(key, out T value))
        {
            return value;
        }

        return null;
    }
}