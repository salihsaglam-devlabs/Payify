using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace LinkPara.HttpProviders.DataContainer;

public class DataContainerProviderService : HttpClientBase, IDataContainerProvider
{

    public DataContainerProviderService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<T> GetDataContainerAsync<T>(string key)
    {
        var response = await GetAsync(key);
        var dataContainer = await response.Content.ReadFromJsonAsync<Models.DataContainer>();
        return JsonConvert.DeserializeObject<T>(dataContainer!.Value);
    }
}