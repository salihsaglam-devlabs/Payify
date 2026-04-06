using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.HttpClients;

public class BankApiHttpClient : HttpClientBase, IBankApiHttpClient
{
    public BankApiHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<BankApiDto> GetByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/BankApis/{id}");
        var bankApi = await response.Content.ReadFromJsonAsync<BankApiDto>();
        return bankApi;
    }

    public async Task<PaginatedList<BankApiDto>> GetListAsync(GetBankApiListRequest request)
    {
        var url = CreateUrlWithParams($"v1/BankApis", request, true);
        var response = await GetAsync(url);
        var bankApiList = await response.Content.ReadFromJsonAsync<PaginatedList<BankApiDto>>();
        return bankApiList;
    }

    public async Task SaveAsync(SaveBankApiRequest request)
    {
        await PostAsJsonAsync($"v1/BankApis", request);
    }

    public async Task UpdateAsync(UpdateBankApiRequest request)
    {
        await PutAsJsonAsync($"v1/BankApis", request);
    }

    public async Task DeleteAsync(DeleteBankApiRequest request)
    {
        await DeleteAsync($"v1/BankApis/{request.Id}");
    }
}
