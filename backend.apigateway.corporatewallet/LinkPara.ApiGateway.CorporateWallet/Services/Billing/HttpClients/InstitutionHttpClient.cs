using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public class InstitutionHttpClient : HttpClientBase, IInstitutionHttpClient
{
    public InstitutionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<InstitutionDto>> GetAllInstitutionAsync(InstitutionFilterRequest request)
    {
        request.RecordStatus = RecordStatus.Active;

        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/Institutions" + queryString);

        return await response.Content.ReadFromJsonAsync<PaginatedList<InstitutionDto>>();
    }
}
