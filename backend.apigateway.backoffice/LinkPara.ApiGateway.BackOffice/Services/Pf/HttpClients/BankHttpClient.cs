using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public class BankHttpClient : HttpClientBase, IBankHttpClient
{
    public BankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<BankDto>> GetAllAsync(GetFilterBankRequest request)
    {
        var url = CreateUrlWithParams($"v1/Banks", request, true);
        var response = await GetAsync(url);
        var banks = await response.Content.ReadFromJsonAsync<PaginatedList<BankDto>>();
        return banks ?? throw new InvalidOperationException();
    }

    public async Task<List<BankApiKeyDto>> GetAllBankApiKeyAsync(Guid acquireBankId)
    {
        var response = await GetAsync($"v1/Banks/{acquireBankId}/bank-api-keys");
        var bankApiKeys = await response.Content.ReadFromJsonAsync<List<BankApiKeyDto>>();
        return bankApiKeys ?? throw new InvalidOperationException();
    }
}
