namespace LinkPara.HttpProviders.DbProvider;

public interface IDatabaseProviderService
{
    Task<string> GetProviderAsync();
}
