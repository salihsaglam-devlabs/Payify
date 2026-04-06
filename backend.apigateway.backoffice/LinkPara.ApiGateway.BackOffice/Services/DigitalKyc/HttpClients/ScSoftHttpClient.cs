using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.HttpClients;
public class ScSoftHttpClient : HttpClientBase, IScSoftHttpClient
{
    public ScSoftHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<CustomerInformationResponse>> GetAllCustomerInformationsAsync(GetAllCustomerInformationsRequest request)
    {
        var url = CreateUrlWithParams("v1/ScSoft/customer-information", request, true);
        var response = await GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PaginatedList<CustomerInformationResponse>>();
    }

    public async Task<CustomerInformationResponse> GetCustomerInformationByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/ScSoft/{id}/customer-information");
        return await response.Content.ReadFromJsonAsync<CustomerInformationResponse>();
    }
}
