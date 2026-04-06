using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;

public class InstitutionHttpClient : HttpClientBase, IInstitutionHttpClient
{
    public InstitutionHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<PaginatedList<InstitutionDto>> GetAllInstitutionAsync(InstitutionFilterRequest request)
    {
        var url = CreateUrlWithParams($"v1/Institutions", request, true);
        var response = await GetAsync(url);

        return await response.Content.ReadFromJsonAsync<PaginatedList<InstitutionDto>>();
    }

    public async Task<InstitutionDto> GetByIdAsync(Guid institutionId)
    {
        var response = await GetAsync($"v1/Institutions/{institutionId}");

        return await response.Content.ReadFromJsonAsync<InstitutionDto>();
    }

    public async Task UpdateAsync(UpdateInstitutionRequest request)
    {
        var response = await PutAsJsonAsync($"v1/Institutions", request);
        if (!response.IsSuccessStatusCode) throw new InvalidOperationException();
    }
}
