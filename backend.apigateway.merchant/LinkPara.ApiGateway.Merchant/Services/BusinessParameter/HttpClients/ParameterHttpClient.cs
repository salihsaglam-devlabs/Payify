using LinkPara.ApiGateway.Merchant.Services.BusinessParameter.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.BusinessParameter.HttpClients;

public class ParameterHttpClient : HttpClientBase, IParameterHttpClient
{
    public ParameterHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<List<ParameterDto>> GetParametersAsync(string groupCode)
    {
        var response = await GetAsync($"v1/Parameters/{groupCode}");
        var parameters = await response.Content.ReadFromJsonAsync<List<ParameterDto>>();
        return parameters ?? throw new InvalidOperationException();
    }
}
