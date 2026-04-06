using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class CompanyIbanHttpClient : HttpClientBase, ICompanyIbanHttpClient
{
    public CompanyIbanHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<CompanyIbanDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/CompanyIbans/{id}");
        return await response.Content.ReadFromJsonAsync<CompanyIbanDto>();
    }

    public async Task<PaginatedList<CompanyIbanDto>> GetListAsync(GetCompanyIbanListRequest request)
    {
        var url = CreateUrlWithParams($"v1/CompanyIbans", request, true);
        var response = await GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PaginatedList<CompanyIbanDto>>();
    }

    public async Task SaveAsync(SaveCompanyIbanRequest request)
    {
        await PostAsJsonAsync($"v1/CompanyIbans", request);
    }

    public async Task UpdateAsync(UpdateCompanyIbanRequest request)
    {
        await PutAsJsonAsync($"v1/CompanyIbans", request);
    }

    public async Task DeleteAsync(DeleteCompanyIbanRequest request)
    {
        await DeleteAsync($"v1/CompanyIbans/{request.Id}");
    }
}
