using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class VendorHttpClient : HttpClientBase, IVendorHttpClient
{
    public VendorHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<VendorDto>> GetAllVendorAsync(VendorFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Vendors", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<VendorDto>>();
    }
}