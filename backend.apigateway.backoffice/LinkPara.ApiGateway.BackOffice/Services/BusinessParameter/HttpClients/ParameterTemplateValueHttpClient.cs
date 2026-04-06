using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.HttpClients
{
    public class ParameterTemplateValueHttpClient : HttpClientBase, IParameterTemplateValueHttpClient
    {
        public ParameterTemplateValueHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {
        }
        public async Task SaveAsync(SaveParameterTemplateValueDto request)
        {
            var response = await PostAsJsonAsync($"v1/ParameterTemplateValues", request);
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task<ParameterTemplateValueDto> GetParameterTemplateValueByIdAsync(Guid id)
        {
            var response = await GetAsync($"v1/ParameterTemplateValues/{id}");
            var parameterTemplateValue = await response.Content.ReadFromJsonAsync<ParameterTemplateValueDto>();
            return parameterTemplateValue ?? throw new InvalidOperationException();
        }
        public async Task<ParameterTemplateValueDto> UpdateAsync(UpdateParameterTemplateValueRequest request)
        {
            var response = await PutAsJsonAsync($"v1/ParameterTemplateValues", request);
            var parameterTemplateValueResponse = await response.Content.ReadFromJsonAsync<ParameterTemplateValueDto>();
            return parameterTemplateValueResponse ?? throw new InvalidOperationException();
        }
        public async Task DeleteParameterTemplateValueAsync(Guid id)
        {
            var response = await DeleteAsync($"v1/ParameterTemplateValues/{id}");
            if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
        }
        public async Task<PaginatedList<ParameterTemplateValueDto>> GetParameterTemplateValuesAsync(GetAllParameterTemplateValueRequest request)
        {
            var url = CreateUrlWithParams($"v1/ParameterTemplateValues", request, true);
            var response = await GetAsync(url);
            var parameterTemplateValues = await response.Content.ReadFromJsonAsync<PaginatedList<ParameterTemplateValueDto>>();
            return parameterTemplateValues ?? throw new InvalidOperationException();
        }
    }
}
