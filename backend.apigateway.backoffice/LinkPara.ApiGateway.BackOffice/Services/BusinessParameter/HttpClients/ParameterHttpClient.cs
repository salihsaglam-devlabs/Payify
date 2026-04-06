using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;

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
    public async Task SaveAsync(SaveParameterDto request)
    {
        var response = await PostAsJsonAsync($"v1/Parameters", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Parameters", request, true);
        var response = await GetAsync(url);
        var parameters = await response.Content.ReadFromJsonAsync<PaginatedList<ParameterDto>>();
        return parameters ?? throw new InvalidOperationException();
    }
    public async Task<ParameterDto> GetParameterByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Parameters/{id}");
        var parameter = await response.Content.ReadFromJsonAsync<ParameterDto>();
        return parameter ?? throw new InvalidOperationException();
    }
    public async Task DeleteParameterAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/Parameters/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<ParameterDto> UpdateAsync(UpdateParameterRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Parameters", request);
        var parameterResponse = await response.Content.ReadFromJsonAsync<ParameterDto>();
        return parameterResponse ?? throw new InvalidOperationException();
    }
}
