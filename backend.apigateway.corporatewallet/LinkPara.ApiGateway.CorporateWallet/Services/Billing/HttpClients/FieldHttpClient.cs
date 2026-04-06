using LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.HttpClients;

public class FieldHttpClient : HttpClientBase, IFieldHttpClient
{
    public FieldHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync(Guid institutionId)
    {
        var response = await GetAsync($"v1/Fields/{institutionId}");

        return await response.Content.ReadFromJsonAsync<PaginatedList<FieldDto>>();
    }
}
