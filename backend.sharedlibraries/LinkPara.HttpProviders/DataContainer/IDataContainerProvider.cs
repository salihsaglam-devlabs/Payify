namespace LinkPara.HttpProviders.DataContainer;

public interface IDataContainerProvider
{        
    Task<T> GetDataContainerAsync<T>(string key);
}