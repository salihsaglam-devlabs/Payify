using LinkPara.ApiGateway.Boa.Services.BusinessParameter.Models.Responses;

namespace LinkPara.ApiGateway.Boa.Services.BusinessParameter.HttpClients;

public class ParameterHttpClient : HttpClientBase, IParameterHttpClient
{
    public ParameterHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }
    
    public async Task<List<ParameterDto>> GetParametersAsync(string groupCode)
    {
        var response = await GetAsync($"v1/BoaParameters/{groupCode}");
        var parameters = await response.Content.ReadFromJsonAsync<List<ParameterDto>>();
        return parameters ?? throw new InvalidOperationException();
    }
}