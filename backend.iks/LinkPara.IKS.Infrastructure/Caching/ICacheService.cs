namespace LinkPara.IKS.Infrastructure.Caching;

public interface ICacheService
{
    void Add<T>(string key, T value, DateTime? absoluteExpiration = null);    
    T Get<T>(string key) where T : class;
}
