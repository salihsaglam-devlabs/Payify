using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;

public class BankHttpClient : HttpClientBase, IBankHttpClient
{
    public BankHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) 
        : base(client, httpContextAccessor)
    {
    }

    public async Task<PaginatedList<BankDto>> GetAllBanksAsync(GetFilterBankRequest request)
    {
        var url = CreateUrlWithParams($"v1/Banks", request, true);
        var response = await GetAsync(url);
        var banks = await response.Content.ReadFromJsonAsync<PaginatedList<BankDto>>();
        return banks ?? throw new InvalidOperationException();
    }
}