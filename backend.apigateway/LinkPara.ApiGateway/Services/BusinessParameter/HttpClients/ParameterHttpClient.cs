using LinkPara.ApiGateway.Services.BusinessParameter.Models.Request;
using LinkPara.ApiGateway.Services.BusinessParameter.Models.Response;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.BusinessParameter.HttpClients
{
    public class ParameterHttpClient : HttpClientBase, IParameterHttpClient
    {
        public ParameterHttpClient(HttpClient client,
            IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {

        }
        public async Task<PaginatedList<ParameterDto>> GetAllParameterAsync(GetAllParameterRequest request)
        {
            var url = CreateUrlWithParams($"v1/Parameters", request, true);
            var response = await GetAsync(url);
            var parameters = await response.Content.ReadFromJsonAsync<PaginatedList<ParameterDto>>();
            return parameters ?? throw new InvalidOperationException();
        }
        public async Task<List<ParameterDto>> GetParametersAsync(string groupCode)
        {
            var response = await GetAsync($"v1/Parameters/{groupCode}");
            var parameters = await response.Content.ReadFromJsonAsync<List<ParameterDto>>();
            return parameters ?? throw new InvalidOperationException();
        }

        public async Task<List<ParameterDto>> GetProfessionParametersAsync()
        {
            var response = await GetAsync($"v1/Parameters/professions");
            var parameters = await response.Content.ReadFromJsonAsync<List<ParameterDto>>();
            return parameters ?? throw new InvalidOperationException();
        }

        public async Task<List<ParameterDto>> GetCompanyInfoParametersAsync()
        {
            var response = await GetAsync($"v1/Parameters/company-info");
            var parameters = await response.Content.ReadFromJsonAsync<List<ParameterDto>>();
            return parameters ?? throw new InvalidOperationException();
        }
    }
}
