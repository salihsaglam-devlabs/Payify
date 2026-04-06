using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
public class ParameterGroupHttpClient : HttpClientBase, IParameterGroupHttpClient
{
    public ParameterGroupHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task SaveAsync(SaveParameterGroupDto request)
    {
        var response = await PostAsJsonAsync($"v1/ParameterGroups", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<PaginatedList<ParameterGroupDto>> GetParameterGroupsAsync(GetAllParameterGroupRequest request)
    {
        var url = CreateUrlWithParams($"v1/ParameterGroups", request, true);
        var response = await GetAsync(url);
        var parameterGroups = await response.Content.ReadFromJsonAsync<PaginatedList<ParameterGroupDto>>();
        return parameterGroups ?? throw new InvalidOperationException();

    }
    public async Task<ParameterGroupDto> GetParameterGroupByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/ParameterGroups/{id}");
        var parameterGroup = await response.Content.ReadFromJsonAsync<ParameterGroupDto>();
        return parameterGroup ?? throw new InvalidOperationException();
    }
    public async Task<ParameterGroupDto> UpdateAsync(UpdateParameterGroupRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParameterGroups", request);
        var parameterGroupResponse = await response.Content.ReadFromJsonAsync<ParameterGroupDto>();
        return parameterGroupResponse ?? throw new InvalidOperationException();
    }
    public async Task DeleteParameterGroupAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/ParameterGroups/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }

}
