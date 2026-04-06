namespace LinkPara.Cache;

public interface ICacheService
{
    void Add<T>(string key, T value, int? expirationInMinutes = null);
    T Get<T>(string key);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> task, int? expirationInMinutes = null);
    Task<bool> ContainsKeyAsync<T>(string key);
}