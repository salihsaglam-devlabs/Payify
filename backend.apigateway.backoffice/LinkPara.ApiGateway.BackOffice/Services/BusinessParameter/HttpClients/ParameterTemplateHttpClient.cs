using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients;
public class ParameterTemplateHttpClient : HttpClientBase, IParameterTemplateHttpClient
{
    public ParameterTemplateHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task SaveAsync(SaveParameterTemplateDto request)
    {
        var response = await PostAsJsonAsync($"v1/ParameterTemplates", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<ParameterTemplateDto> GetParameterTemplateByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/ParameterTemplates/{id}");
        var parameterTemplate = await response.Content.ReadFromJsonAsync<ParameterTemplateDto>();
        return parameterTemplate ?? throw new InvalidOperationException();
    }
    public async Task<ParameterTemplateDto> UpdateAsync(UpdateParameterTemplateRequest request)
    {
        var response = await PutAsJsonAsync($"v1/ParameterTemplates", request);
        var parameterTemplateResponse = await response.Content.ReadFromJsonAsync<ParameterTemplateDto>();
        return parameterTemplateResponse ?? throw new InvalidOperationException();
    }
    public async Task DeleteParameterTemplateAsync(Guid id)
    {
        var response = await DeleteAsync($"v1/ParameterTemplates/{id}");
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
    public async Task<PaginatedList<ParameterTemplateDto>> GetParameterTemplatesAsync(GetAllParameterTemplateRequest request)
    {
        var url = CreateUrlWithParams($"v1/ParameterTemplates", request, true);
        var response = await GetAsync(url);
        var parameterTemplates = await response.Content.ReadFromJsonAsync<PaginatedList<ParameterTemplateDto>>();
        return parameterTemplates ?? throw new InvalidOperationException();
    }
    public async Task<List<ParameterTemplateDto>> GetParameterTemplatesByGroupCodeAsync(string groupCode)
    {
        var response = await GetAsync($"v1/ParameterTemplates/{groupCode}");
        var parameterTemplate = await response.Content.ReadFromJsonAsync<List<ParameterTemplateDto>>();
        return parameterTemplate ?? throw new InvalidOperationException();
    }
}

