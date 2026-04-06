namespace LinkPara.Template.Infrastructure.Caching;

public interface ICacheService
{
    void Add<T>(string key, T value, DateTimeOffset? absoluteExpiration = null);    
    T Get<T>(string key) where T : class;
}
